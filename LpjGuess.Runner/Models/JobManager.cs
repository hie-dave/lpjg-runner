namespace LpjGuess.Runner.Models;

/// <summary>
/// Manages the execution of LPJ-Guess jobs.
/// </summary>
public class JobManager
{
	private readonly RunSettings settings;

	/// <summary>
	/// Create a new <see cref="JobManager"/> instance.
	/// </summary>
	/// <param name="settings">Run settings.</param>
	public JobManager(RunSettings settings)
	{
		this.settings = settings;
	}

	/// <summary>
	/// Run all of the jobs.
	/// </summary>
	/// <param name="jobs">Jobs to be run.</param>
	/// <param name="ct">Cancellation token.</param>
	public async Task RunAllAsync(IEnumerable<Job> jobs, CancellationToken ct)
	{
		if (settings.DryRun)
		{
			Console.WriteLine("Dry run - jobs would be executed");
			return;
		}

		if (settings.ParallelProcessing)
		{
			jobs = jobs.AsParallel()
				.WithDegreeOfParallelism(settings.CpuCount)
				.WithCancellation(ct);
		}

		await Parallel.ForEachAsync(
			jobs,
			new ParallelOptions
			{
				MaxDegreeOfParallelism = settings.CpuCount,
				CancellationToken = ct
			},
			async (job, ct) =>
			{
				IRunner runner = CreateRunner(job.Name);
				await runner.Run(job.InsFile, ct);
			});
	}

	/// <summary>
	/// Create a runner for the job.
	/// </summary>
	/// <param name="jobName">Name of the job.</param>
	private IRunner CreateRunner(string jobName)
	{
		return settings.RunLocal 
			? new LocalRunner(settings) 
			: new PbsRunner(jobName, settings);
	}
}
