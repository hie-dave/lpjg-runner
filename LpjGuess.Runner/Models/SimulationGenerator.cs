using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using LpjGuess.Runner.Extensions;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
/// <remarks>
/// Refactor most of the logic out of here.
/// </remarks>
public class SimulationGenerator
{
	// /// <summary>
	// /// List of paths to instruction files to be run.
	// /// </summary>
	// public IReadOnlyCollection<string> InsFiles { get; private init; }

	// /// <summary>
	// /// List of PFTs to be enabled for this run. All others will be disabled.
	// /// If empty, no PFTs will be disabled.
	// /// </summary>
	// public IReadOnlyCollection<string> Pfts { get; private init; }

	// /// <summary>
	// /// The parameter changes being applied in this run.
	// /// </summary>
	// public IReadOnlyCollection<Factorial> Factorials { get; private init; }

	/// <summary>
	/// Run settings.
	/// </summary>
	public Configuration Settings { get; private init; }

	/// <summary>
	/// Create a new <see cref="SimulationGenerator"/> instance.
	/// </summary>
	/// <param name="insFiles">List of paths to instruction files to be run.</param>
	/// <param name="pfts">List of PFTs to be enabled for this run. All others will be disabled. If empty, no PFTs will be disabled.</param>
	/// <param name="parameters">The parameter changes being applied in this run.</param>
	/// <param name="settings">Run settings.</param>
	public SimulationGenerator(Configuration settings)
	{
		Settings = settings;
	}

	/// <summary>
	/// Generate all jobs for these instructions.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public async Task<IEnumerable<Job>> GenerateAllJobsAsync(CancellationToken ct)
	{
		IEnumerable<RunConfig> query = Settings.Runs;
		if (Settings.Global.Parallel)
		{
			query = query.AsParallel()
						 .WithDegreeOfParallelism(Settings.Global.CpuCount)
						 .WithCancellation(ct);
		}

		return await query.SelectManyAsync(run => GenerateJobsAsync(run, ct));
	}

	/// <summary>
	/// Generate all jobs to be run for the specified instruction file.
	/// </summary>
	/// <param name="run">The run configuration.</param>
	private async Task<IEnumerable<Job>> GenerateJobsAsync(RunConfig run, CancellationToken ct)
	{
		IEnumerable<Factorial> query = run.GenerateFactorials().ToList();
		int nfactorial = query.Count();

		if (Settings.Global.Parallel)
		{
			query = query.AsParallel()
			 			 .WithDegreeOfParallelism(Settings.Global.CpuCount)
						 .WithCancellation(ct);
		}

		return await query.SelectManyAsync(f => GenerateSimulationsAsync(run, f, nfactorial));
	}

	private async Task<IEnumerable<Job>> GenerateSimulationsAsync(RunConfig run, Factorial factorial, int nfactorial)
	{
		// Apply changes from this factorial.
		List<Job> jobs = new();
		foreach (string insFile in run.InsFiles)
		{
			string jobName = factorial.GetName();
			// If total number of ins files > 1, use base name plus factorial
			// name in order to distinguish between them.
			if (Settings.Runs.Sum(r => r.InsFiles.Length) > 1)
				jobName = $"{Path.GetFileNameWithoutExtension(insFile)}-{jobName}";
			string targetInsFile = await ApplyOverridesAsync(factorial, insFile, jobName, nfactorial, run.InsFiles.Length, run.Pfts);
			jobs.Add(new Job(jobName, targetInsFile));
		}

		return jobs;
	}

	private async Task<string> ApplyOverridesAsync(Factorial factorial, string insFile, string name, int nfactorial, int nins, string[] pfts)
	{
		string basename = Path.GetFileNameWithoutExtension(insFile);
		string ext = Path.GetExtension(insFile);
		string jobDirectory = Settings.Global.OutputDirectory;
		if (nins > 1 && nfactorial > 1)
			jobDirectory = Path.Combine(jobDirectory, basename);
		jobDirectory = Path.Combine(jobDirectory, name);
		string targetInsFile = Path.Combine(jobDirectory, $"{basename}-{name}{ext}");
		Directory.CreateDirectory(jobDirectory);

		try
		{
			InstructionFileParser ins = InstructionFileParser.FromFile(insFile);

			foreach (Factor factor in factorial.Factors)
				ins.ApplyChange(factor);

			// Disable all PFTs except those required.
			if (pfts.Length > 0)
			{
				ins.DisableAllPfts();
				foreach (string pft in pfts)
					ins.EnablePft(pft);
			}

			string content = ins.GenerateContent();
			await File.WriteAllTextAsync(targetInsFile, content);

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

	private void CleanupFailure(Exception error, string file)
	{
		Console.Error.WriteLine($"WARNING: Failed to clean temporary file: '{file}':");
		Console.Error.WriteLine(error);
	}
}
