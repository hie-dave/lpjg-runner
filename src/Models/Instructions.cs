using System.Diagnostics;
using System.Globalization;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
/// <remarks>
/// Refactor most of the logic out of here.
/// </remarks>
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
		Settings = settings;
		if (factorials.Count == 0)
			factorials = new List<Factorial>() { new Factorial(new List<Factor>()) };
		Factorials = factorials;
	}

	/// <summary>
	/// Run all of the factorial simulations.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public async Task RunAll(CancellationToken ct)
	{
		IReadOnlyList<IJob> jobs = GetJobs(ct);
		JobManager jobManager = jobManager(Settings.LocalCpuCount);
		await jobManager.RunAll(jobs);
	}

	private IReadOnlyList<IJob> GetJobs(CancellationToken ct)
	{
		// Record current working directory, and cd back to here after running
		// each file.
		string cwd = Directory.GetCurrentDirectory();

		List<IJob> allRunners = new List<IJob>();
		Parallel.ForEach(InsFiles, insFile =>
		{
			try
			{
				IEnumerable<IJob> runners = RunFile(insFile, ct);
				ct.ThrowIfCancellationRequested();
				lock(allRunners)
					allRunners.AddRange(runners);
			}
			finally
			{
				Directory.SetCurrentDirectory(cwd);
			}
		});
		return allRunners;
	}

	/// <summary>
	/// Run all factorials for the specified instruction file.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	/// <param name="ct">Cancellation token.</param>
	private IReadOnlyList<IJob> RunFile(string insFile, CancellationToken ct)
	{
		List<IJob> runners = new List<IJob>();

		// Run each factorial one by one.
		// foreach (Factorial factorial in Factorials)
		Parallel.ForEach(Factorials, factorial =>
		{
			IJob runner = RunFactorial(insFile, factorial, ct);
			lock (runners)
				runners.Add(runner);
		});
		return runners;
	}

	private IJob RunFactorial(string insFile, Factorial factorial, CancellationToken ct)
	{
		// Apply changes from this factorial.
		ct.ThrowIfCancellationRequested();
		string jobName = $"{Path.GetFileNameWithoutExtension(insFile)}-{factorial.GetName()}";
		string targetInsFile = ApplyOverrides(factorial, insFile, jobName);

		// Run this factorial (well, submit the job for running).
		ct.ThrowIfCancellationRequested();

		return CreateRunner(targetInsFile, jobName);
	}

	/// <summary>
	/// Report a cleanup failure in such a way that doesn't raise another exception.
	/// </summary>
	/// <param name="error">The error.</param>
	private void CleanupFailure(Exception error, string file)
	{
		Console.Error.WriteLine($"WARNING: Failed to clean temporary file: '{file}':");
		Console.Error.WriteLine(error);
	}

	/// <summary>
	/// Apply the specified changes to the instruction file, without modifying
	/// the original instruction file. Returns the path to a new instruction
	/// file which contains the changes.
	/// </summary>
	/// <param name="factorial">Changes to be applied to the .ins file.</param>
	/// <param name="insFile">Path to an instruction file.</param>
	/// <param name="name">Unique name given to this factorial run.</param>
	private string ApplyOverrides(Factorial factorial, string insFile, string name)
	{
		string file = Path.GetFileNameWithoutExtension(insFile);
		string ext = Path.GetExtension(insFile);
		string jobDirectory = Path.Combine(Settings.OutputDirectory, Settings.JobName, name);
		string targetInsFile = Path.Combine(jobDirectory, $"{file}-{name}{ext}");
		Directory.CreateDirectory(jobDirectory);

		try
		{
			// Apply changes to the instruction file as required.
			foreach (Factor factor in factorial.Factors)
				ins.ApplyChange(factor);
			ins.SaveChanges(targetInsFile);
			return targetInsFile;
		}
		catch (Exception error)
		{
			try
			{
				if (File.Exists(targetInsFile))
					File.Delete(targetInsFile);
			}
			catch (Exception nested)
			{
				CleanupFailure(nested, targetInsFile);
			}
			throw new Exception($"Failed to apply overrides to file '{insFile}'", error);
		}	
	}

	private IJob CreateRunner(string insFile, string jobName)
	{
		if (Settings.RunLocal)
			return new LocalRunner(insFile, Settings);
		return new PbsRunner(insFile, jobName, Settings);
	}
}
