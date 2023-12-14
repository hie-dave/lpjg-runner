namespace LpjGuess.Runner.Models;

/// <summary>
/// Describes settings for a run.
/// </summary>
public class RunSettings
{
	/// <summary>
	/// Iff true, the run directory will be created but the job will not be
	/// submitted.
	/// </summary>
	public bool DryRun { get; private init; }

	/// <summary>
	/// If true, will run the model in the local environment. If false, will
	/// submit the jobs to PBS.
	/// </summary>
	public bool RunLocal { get; private init; }

	/// <summary>
	/// Output directory of the run.
	/// </summary>
	public string OutputDirectory { get; private init; }

	/// <summary>
	/// Path to the LPJ-Guess executable.
	/// </summary>
	public string GuessPath { get; private init; }

	/// <summary>
	/// Input module to be used by LPJ-Guess.
	/// </summary>
	public string InputModule { get; private init; }

	/// <summary>
	/// Number of CPUs to use in the local environment for job submission or
	/// local runs.
	/// </summary>
	public uint LocalCpuCount { get; private init; }

	/// <summary>
	/// Number of CPUs to be allocated to the PBS job.
	/// </summary>
	public uint CpuCount { get; private init; }

	/// <summary>
	/// Maximum walltime allowed for the job.
	/// </summary>
	public TimeSpan Walltime { get; private init; }

	/// <summary>
	/// Amount of memory to be allocated to the job.
	/// </summary>
	public uint Memory { get; private init; }

	/// <summary>
	/// Queue to which the job shoudl be submitted.
	/// </summary>
	public string Queue { get; private init; }

	/// <summary>
	/// PBS project under which the job should be submitted.
	/// </summary>
	public string Project { get; private init; }

	/// <summary>
	/// True to enable email notifications for the job, false otherwise.
	/// </summary>
	public bool EmailNotifications { get; private init; }

	/// <summary>
	/// Email address to be used for the job. Only used if
	/// <see cref="EmailNotifications"/> is true.
	/// </summary>
	public string EmailAddress { get; private init; }

	/// <summary>
	/// Name of the job.
	/// </summary>
	public string JobName { get; private init; }

	/// <summary>
	/// Create a new <see cref="RunSettings"/> instance.
	/// </summary>
	/// <param name="dryRun">Iff true, the run directory will be created but the job will not be submitted.</param>
	/// <param name="runLocal">True to run simulations in the local environment. False to submit them to PBS.</param>
	/// <param name="outputDirectory">Output directory of the run.</param>
	/// <param name="guessPath">Path to the LPJ-Guess executable.</param>
	/// <param name="inputModule">Input module to be used by LPJ-Guess.</param>
	/// <param name="localCpuCount">Number of CPUs to use in the local environment for job submission or local runs.</param>
	/// <param name="cpuCount">Number of CPUs to be allocated to the job.</param>
	/// <param name="walltime">Maximum walltime allowed for the job.</param>
	/// <param name="memory">Amount of memory to be allocated to the job.</param>
	/// <param name="queue">Queue to which the job shoudl be submitted.</param>
	/// <param name="project">PBS project under which the job should be submitted.</param>
	/// <param name="emailNotifications">True to enable email notifications for the job, false otherwise.</param>
	/// <param name="emailAddress">Email address to be used for the job. Only used if emailNotifications is true.</param>
	/// <param name="jobName">Name of the job.</param>
	public RunSettings(bool dryRun, bool runLocal, string outputDirectory, string guessPath, string inputModule, uint localCpuCount, uint cpuCount, TimeSpan walltime, uint memory, string queue, string project, bool emailNotifications, string emailAddress, string jobName)
	{
		DryRun = dryRun;
		RunLocal = runLocal;
		OutputDirectory = outputDirectory;
		GuessPath = guessPath;
		InputModule = inputModule;
		LocalCpuCount = localCpuCount;
		CpuCount = cpuCount;
		Walltime = walltime;
		Memory = memory;
		Queue = queue;
		Project = project;
		EmailNotifications = emailNotifications;
		EmailAddress = emailAddress;
		JobName = jobName;
	}
}
