using LpjGuess.Runner.Models.Raw;
using Tomlyn.Helpers;

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
	/// Number of CPUs to be allocated to the job.
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
	/// <param name="outputDirectory">Output directory of the run.</param>
	/// <param name="guessPath">Path to the LPJ-Guess executable.</param>
	/// <param name="inputModule">Input module to be used by LPJ-Guess.</param>
	/// <param name="cpuCount">Number of CPUs to be allocated to the job.</param>
	/// <param name="walltime">Maximum walltime allowed for the job.</param>
	/// <param name="memory">Amount of memory to be allocated to the job.</param>
	/// <param name="queue">Queue to which the job shoudl be submitted.</param>
	/// <param name="project">PBS project under which the job should be submitted.</param>
	/// <param name="emailNotifications">True to enable email notifications for the job, false otherwise.</param>
	/// <param name="emailAddress">Email address to be used for the job. Only used if emailNotifications is true.</param>
	/// <param name="jobName">Name of the job.</param>
	public RunSettings(bool dryRun, string outputDirectory, string guessPath, string inputModule, uint cpuCount, TimeSpan walltime, uint memory, string queue, string project, bool emailNotifications, string emailAddress, string jobName)
	{
		DryRun = dryRun;
		OutputDirectory = outputDirectory;
		GuessPath = guessPath;
		InputModule = inputModule;
		CpuCount = cpuCount;
		Walltime = walltime;
		Memory = memory;
		Queue = queue;
		Project = project;
		EmailNotifications = emailNotifications;
		EmailAddress = emailAddress;
		JobName = jobName;
	}

	/// <summary>
	/// Attempt to create a new <see cref="RunSettings"/> instance from the raw
	/// user input object. This may throw if user inputs are invalid.
	/// </summary>
	/// <param name="model">User input.</param>
	public RunSettings(InputModel model)
	{
		DryRun = model.DryRun;
		EmailNotifications = model.EmailNotifications;

		if (model.CpuCount == 0)
			throw new InvalidOperationException(VarNotSet(nameof(CpuCount)));
		CpuCount = model.CpuCount;

		if (model.Memory == 0)
			throw new InvalidOperationException(VarNotSet(nameof(Memory)));
		Memory = model.Memory;

		if (string.IsNullOrEmpty(model.OutputDirectory))
			throw new InvalidOperationException(VarNotSet(nameof(OutputDirectory)));
		OutputDirectory = model.OutputDirectory;

		if (string.IsNullOrEmpty(model.GuessPath))
			throw new InvalidOperationException(VarNotSet(nameof(GuessPath)));
		GuessPath = model.GuessPath;

		if (string.IsNullOrEmpty(model.InputModule))
			throw new InvalidOperationException(VarNotSet(nameof(InputModule)));
		InputModule = model.InputModule;

		if (string.IsNullOrEmpty(model.Walltime))
			throw new InvalidOperationException(VarNotSet(nameof(Walltime)));
		if (!TimeSpan.TryParseExact(model.Walltime, "c", null, out TimeSpan walltime))
			throw new InvalidOperationException($"Invalid walltime value: '${model.Walltime}' in input file");
		Walltime = walltime;

		if (string.IsNullOrEmpty(model.Queue))
			throw new InvalidOperationException(VarNotSet(nameof(Queue)));
		Queue = model.Queue;

		if (string.IsNullOrEmpty(model.Project))
			throw new InvalidOperationException(VarNotSet(nameof(Project)));
		Project = model.Project;

		if (model.EmailNotifications && string.IsNullOrEmpty(model.EmailAddress))
			throw new InvalidOperationException(VarNotSet(nameof(EmailAddress)));

		// If EmailNotifications is false, then we can use an empty string here,
		// as EmailAddress won't be used.
		EmailAddress = model.EmailAddress ?? "";

		if (string.IsNullOrEmpty(model.JobName))
			throw new InvalidOperationException(VarNotSet(nameof(JobName)));
		JobName = model.JobName;
	}

	/// <summary>
	/// Return an appropriate error message for when a variable is not set in
	/// the input file.
	/// </summary>
	/// <param name="variable">Name of the variable.</param>
	private string VarNotSet(string variable)
	{
		string name = TomlNamingHelper.PascalToSnakeCase(variable);
		return $"Variable '{name}' is not set in input file";
	}
}
