namespace LpjGuess.Runner.Models.Raw;

/// <summary>
/// The deserialized model type.
/// </summary>
public class InputModel
{
	/// <summary>
	/// List of pft names to be used in the runs.
	/// </summary>
	public List<string> Pfts { get; set; } = new List<string>();

	/// <summary>
	/// Instruction files to be run.
	/// </summary>
	public List<string> Insfiles { get; set; } = new List<string>();

	/// <summary>
	/// Parameter changes for the runs.
	/// </summary>
	public Dictionary<string, List<string>> Parameters { get; set; } = new Dictionary<string, List<string>>();

	/// <summary>
	/// Iff true, the run directory will be created but the job will not be
	/// submitted.
	/// </summary>
	public bool DryRun { get; set; }

	/// <summary>
	/// Output directory of the run.
	/// </summary>
	public string? OutputDirectory { get; set; }

	/// <summary>
	/// Path to the LPJ-Guess executable.
	/// </summary>
	public string? GuessPath { get; set; }

	/// <summary>
	/// Input module to be used by LPJ-Guess.
	/// </summary>
	public string? InputModule { get; set; }

	/// <summary>
	/// Number of CPUs to be allocated to the job.
	/// </summary>
	public uint CpuCount { get; set; }

	/// <summary>
	/// Maximum walltime allowed for the job.
	/// </summary>
	public string? Walltime { get; set; }

	/// <summary>
	/// Amount of memory (in GB) to be allocated to the job.
	/// </summary>
	public uint Memory { get; set; }

	/// <summary>
	/// Queue to which the job shoudl be submitted.
	/// </summary>
	public string? Queue { get; set; }

	/// <summary>
	/// PBS project under which the job should be submitted.
	/// </summary>
	public string? Project { get; set; }

	/// <summary>
	/// True to enable email notifications for the job, false otherwise.
	/// </summary>
	public bool EmailNotifications { get; set; }

	/// <summary>
	/// Email address to be used for the job. Only used if
	/// <see cref="EmailNotifications"/> is true.
	/// </summary>
	public string? EmailAddress { get; set; }

	/// <summary>
	/// Name of the job.
	/// </summary>
	public string? JobName { get; set; }
}
