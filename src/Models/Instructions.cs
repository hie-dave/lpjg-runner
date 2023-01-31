using System.Diagnostics;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
public class Instructions
{
	/// <summary>
	/// List of paths to instruction files to be run.
	/// </summary>
	public IReadOnlyCollection<string> InsFiles { get; private init; }

	/// <summary>
	/// List of PFTs to be enabled for this run. All others will be disabled.
	/// If empty, no PFTs will be disabled.
	/// </summary>
	public IReadOnlyCollection<string> Pfts { get; private init; }

	/// <summary>
	/// The parameter changes being applied in this run.
	/// </summary>
	public IReadOnlyCollection<Factorial> Factorials { get; private init; }

	/// <summary>
	/// Run settings.
	/// </summary>
	public RunSettings Settings { get; private init; }

	/// <summary>
	/// Create a new <see cref="Instructions"/> instance.
	/// </summary>
	/// <param name="insFiles">List of paths to instruction files to be run.</param>
	/// <param name="pfts">List of PFTs to be enabled for this run. All others will be disabled. If empty, no PFTs will be disabled.</param>
	/// <param name="parameters">The parameter changes being applied in this run.</param>
	/// <param name="settings">Run settings.</param>
	public Instructions(IReadOnlyCollection<string> insFiles, IReadOnlyCollection<string> pfts
		, IReadOnlyCollection<Factorial> factorials, RunSettings settings)
	{
		InsFiles = insFiles;
		Pfts = pfts;
		Factorials = factorials;
		Settings = settings;
	}

	/// <summary>
	/// Run all of the factorial simulations.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public async Task RunAll(CancellationToken ct)
	{
		// Record current working directory, and cd back to here after running
		// each file.
		string cwd = Directory.GetCurrentDirectory();

		foreach (string insFile in InsFiles)
		{
			try
			{
				await RunFile(insFile, ct);
				ct.ThrowIfCancellationRequested();
			}
			finally
			{
				Directory.SetCurrentDirectory(cwd);
			}
		}
	}

	/// <summary>
	/// Run all factorials for the specified instruction file.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	/// <param name="ct">Cancellation token.</param>
	private async Task RunFile(string insFile, CancellationToken ct)
	{
		// Get directory of .ins file and cd into that directory, so that
		// resolution of relative paths in the .ins file can succeed.
		string directory = Path.GetDirectoryName(insFile) ?? Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory(directory);

		// Run each factorial one by one.
		foreach (Factorial factorial in Factorials)
		{
			// Apply changes from this factorial.
			ct.ThrowIfCancellationRequested();
			string targetInsFile = ApplyOverrides(factorial, insFile);

			try
			{
				// Run this factorial (well, submit the job for running).
				ct.ThrowIfCancellationRequested();

				string confFile = await GenerateConfFile(insFile);
				ct.ThrowIfCancellationRequested();

				await Submit(targetInsFile, confFile, ct);
			}
			finally
			{
				// Try to clean up after ourselves, regardless of whether the
				// run succeeded or not.
				try
				{
					if (File.Exists(targetInsFile))
						File.Delete(targetInsFile);
				}
				catch (Exception error)
				{
					Console.Error.WriteLine($"WARNING: Failed to clean temporary file: '{targetInsFile}':");
					Console.Error.WriteLine(error);
				}
			}
		}
	}

	/// <summary>
	/// Apply the specified changes to the instruction file, without modifying
	/// the original instruction file. Returns the path to a new instruction
	/// file which contains the changes.
	/// </summary>
	/// <param name="factorial">Changes to be applied to the .ins file.</param>
	/// <param name="insFile">Path to an instruction file.</param>
	private string ApplyOverrides(Factorial factorial, string insFile)
	{
		string file = Path.GetFileNameWithoutExtension(insFile);
		string ext = Path.GetExtension(insFile);
		string targetInsFile = Path.Combine(Directory.GetCurrentDirectory(), $"{file}-{Guid.NewGuid()}{ext}");

		File.Copy(insFile, targetInsFile);

		try
		{
			InstructionFile ins = new InstructionFile(targetInsFile);
			ins.Flatten();

			// Apply changes to the instruction file as required.
			foreach (Factor factor in factorial.Factors)
				ins.ApplyChange(factor);
			ins.SaveChanges();
			return targetInsFile;
		}
		catch (Exception error)
		{
			try
			{
				File.Delete(targetInsFile);
			}
			catch (Exception nested)
			{
				Console.WriteLine($"WARNING: Failed to clean temporary file: '{targetInsFile}':");
				Console.WriteLine(nested);
			}
			throw new Exception($"Failed to apply overrides to file '{insFile}'", error);
		}	
	}

	/// <summary>
	/// Submit the specified .ins file for running.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	/// <param name="confFile">Path to the .conf file for the run.</param>
	/// <param name="ct">Cancellation token.</param>
	private async Task Submit(string insFile, string confFile, CancellationToken ct)
	{
		const string submitScript = "submit_to_gadi.sh";

		try
		{
			using (Process proc = new Process())
			{
				proc.StartInfo.FileName = submitScript;
				proc.StartInfo.ArgumentList.Add("-s");
				proc.StartInfo.ArgumentList.Add(confFile);
				if (Settings.DryRun)
					proc.StartInfo.ArgumentList.Add("-d");

				// Run the submit script.
				proc.Start();
				await proc.WaitForExitAsync(ct);
				if (ct.IsCancellationRequested && !proc.HasExited)
					proc.Kill();
				ct.ThrowIfCancellationRequested();
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to run submit script for .ins file '{insFile}'", error);
		}
	}

	/// <summary>
	/// Generate the config file which may be passed to the submit script.
	/// </summary>
	/// <param name="insFile">Path to the .ins file.</param>
	private async Task<string> GenerateConfFile(string insFile)
	{
		try
		{
			// Key names.
			const string binary = "BINARY";
			const string nprocess = "NPROCESS";
			const string walltime = "WALLTIME";
			const string memory = "MEMORY";
			const string queue = "QUEUE";
			const string project = "PROJECT";
			const string email = "EMAIL";
			const string emailNotifications = "EMAIL_NOTIFICATIONS";
			const string jobName = "JOB_NAME";
			const string outDir = "OUT_DIR";
			const string insfile = "INSFILE";
			const string inputModule = "INPUT_MODULE";		

			string confFile = Path.GetTempFileName();

			using (Stream stream = File.OpenWrite(confFile))
			using (TextWriter writer = new StreamWriter(stream))
			{
				await Write(writer, binary, Settings.GuessPath);
				await Write(writer, nprocess, Settings.CpuCount);
				await Write(writer, walltime, Settings.Walltime);
				await Write(writer, memory, $"{Settings.Memory:CultureInfo.InvariantCulture}GB");
				await Write(writer, queue, Settings.Queue);
				await Write(writer, project, Settings.Project);
				await Write(writer, email, Settings.EmailAddress);
				await Write(writer, emailNotifications, Settings.EmailNotifications ? "1" : "0");
				await Write(writer, jobName, Settings.JobName);
				await Write(writer, outDir, Settings.OutputDirectory);
				await Write(writer, insfile, insFile);
				await Write(writer, inputModule, Settings.InputModule);
			}
			return confFile;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to generate .conf file for .ins file '{insFile}'", error);
		}
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, TimeSpan value)
	{
		string s = $"{value.Hours:D2}:{value.Minutes:D2}:{value.Seconds:D2}";
		await writer.WriteLineAsync($"{key}={s}");
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, uint value)
	{
		await writer.WriteLineAsync($"{key}={value:CultureInfo.InvariantCulture}");
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, int value)
	{
		await writer.WriteLineAsync($"{key}={value:CultureInfo.InvariantCulture}");
	}

	/// <summary>
	/// Write a shell-quoted key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, string value)
	{
		await writer.WriteLineAsync($"{key}=\"{value}\"");
	}
}
