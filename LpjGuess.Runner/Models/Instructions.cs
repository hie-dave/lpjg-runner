using System.Diagnostics;
using System.Globalization;
using LpjGuess.Runner.Parsers;

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
		List<Task> tasks = new List<Task>();
		Action<string> callback = insFile =>
		{
			Task task = RunFile(insFile, ct);
			lock (tasks)
				tasks.Add(task);
		};

		if (Settings.ParallelProcessing)
			Parallel.ForEach(InsFiles, callback);
		else
			InsFiles.ToList().ForEach(callback);

		foreach (Task task in tasks)
			await task;
	}

	/// <summary>
	/// Run all factorials for the specified instruction file.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	/// <param name="ct">Cancellation token.</param>
	private async Task RunFile(string insFile, CancellationToken ct)
	{
		List<Task> tasks = new List<Task>();

		// Run each factorial one by one.
		// foreach (Factorial factorial in Factorials)
		Action<Factorial> callback = factorial =>
		{
			Task task = RunFactorial(insFile, factorial, ct);
			lock (tasks)
				tasks.Add(task);
		};

		if (Settings.ParallelProcessing)
			Parallel.ForEach(Factorials, callback);
		else
			Factorials.ToList().ForEach(callback);

		foreach (Task task in tasks)
			await task;
	}

	private async Task RunFactorial(string insFile, Factorial factorial, CancellationToken ct)
	{
		// Apply changes from this factorial.
		ct.ThrowIfCancellationRequested();
		string jobName = factorial.GetName();
		if (InsFiles.Count > 1)
			jobName = $"{Path.GetFileNameWithoutExtension(insFile)}-{jobName}";
		string targetInsFile = ApplyOverrides(factorial, insFile, jobName);

		// Run this factorial (well, submit the job for running).
		ct.ThrowIfCancellationRequested();

		IRunner runner = CreateRunner(jobName);
		await runner.Run(targetInsFile, ct);
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
		string jobDirectory = Path.Combine(Settings.OutputDirectory, name);
		string targetInsFile = Path.Combine(jobDirectory, $"{file}-{name}{ext}");
		Directory.CreateDirectory(jobDirectory);

		try
		{
			InstructionFileParser ins = InstructionFileParser.FromFile(insFile);

			// Apply changes to the instruction file as required.
			foreach (Factor factor in factorial.Factors)
				ins.ApplyChange(factor);

			// Save the output file.
			string content = ins.GenerateContent();
			File.WriteAllText(targetInsFile, content);

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

	private IRunner CreateRunner(string jobName)
	{
		if (Settings.RunLocal)
			return new LocalRunner(Settings);
		return new PbsRunner(jobName, Settings);
	}
}
