namespace LpjGuess.Runner.Models;

/// <summary>
/// Manages the execution of LPJ-Guess jobs.
/// </summary>
public class JobManager
{
	/// <summary>
	/// Configuration parameters for the run.
	/// </summary>
	private readonly RunSettings settings;

	/// <summary>
	/// The jobs managed by this job manager instance.
	/// </summary>
	private readonly IReadOnlyList<Job> jobs;

	/// <summary>
	/// Dictionary mapping job names, to those jobs' progres percentages.
	/// </summary>
	private readonly IDictionary<string, int> jobProgress;

	/// <summary>
	/// Start time of the job manager.
	/// </summary>
	private readonly DateTime startTime;

	/// <summary>
	/// The last time that a progress message was written.
	/// </summary>
	private DateTime lastUpdate;

	/// <summary>
	/// Create a new <see cref="JobManager"/> instance.
	/// </summary>
	/// <param name="settings">Run settings.</param>
	public JobManager(RunSettings settings, IEnumerable<Job> jobs)
	{
		this.jobs = jobs.ToList();
		this.settings = settings;
		jobProgress = new Dictionary<string, int>();
		startTime = DateTime.Now;
		lastUpdate = DateTime.MinValue;
	}

	/// <summary>
	/// Run all of the jobs.
	/// </summary>
	/// <param name="jobs">Jobs to be run.</param>
	/// <param name="ct">Cancellation token.</param>
	public async Task RunAllAsync(CancellationToken ct)
	{
		if (settings.DryRun)
		{
			Console.WriteLine("Dry run - jobs would be executed");
			return;
		}

		// Set progress to 0 for all jobs. If we don't do this, only those jobs
		// which have run or are running will exist in jobProgress.
		foreach (var job in jobs)
			jobProgress[job.Name] = 0;

		await Parallel.ForEachAsync(
			jobs,
			new ParallelOptions
			{
				MaxDegreeOfParallelism = settings.CpuCount,
				CancellationToken = ct
			},
			RunJobAsync);

		// Clear progress line when done.
		WriteProgress();
		Console.WriteLine();
	}

	/// <summary>
	/// Run a job asynchronously.
	/// </summary>
	/// <param name="job">The job to run.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	private async ValueTask RunJobAsync(Job job, CancellationToken cancellationToken)
	{
		IRunner runner = CreateRunner(job.Name);
		runner.ProgressChanged += HandleProgress;
		try
		{
			await runner.RunAsync(job, cancellationToken);
		}
		finally
		{
			runner.ProgressChanged -= HandleProgress;
		}
	}

	/// <summary>
	/// Handle a progress message written by a job and intercepted by a job
	/// runner.
	/// </summary>
	/// <param name="sender">A job runner instance.</param>
	/// <param name="e">The event data.</param>
	private void HandleProgress(object? sender, ProgressEventArgs e)
	{
		lock (jobProgress)
		{
			jobProgress[e.JobName] = e.Percentage;

			// Only update if at least 1 second passed
			if ((DateTime.Now - lastUpdate).TotalSeconds >= 1)
			{
				lastUpdate = DateTime.Now;
				WriteProgress();
			}
		}
	}

	/// <summary>
	/// Write a message containing the current progress toward completion.
	/// </summary>
	private void WriteProgress()
	{
		// Calculate aggregate progress
		int total = jobProgress.Values.Sum();
		double percent = jobProgress.Count > 0 ? 1.0 * total / jobProgress.Count : 0;
		double progress = percent / 100.0; // Rescale 0-1
		if (progress < 1e-3)
		{
			Console.Write($"\r{percent:f2}% complete");
			return;
		}

		int ncomplete = jobProgress.Values.Count(c => c == 100);

		TimeSpan elapsed = DateTime.Now - startTime;
		TimeSpan totalTime = elapsed / progress;
		TimeSpan remaining = totalTime - elapsed;

		Console.Write($"\r{percent:f2}% complete, {elapsed:hh\\:mm\\:ss} elapsed, {remaining:hh\\:mm\\:ss} remaining ({ncomplete}/{jobs.Count} simulations complete)");
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
