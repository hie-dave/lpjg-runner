namespace LpjGuess.Runner.Models;

public class PbsSettings
{
    public PbsSettings()
    {
        Walltime = TimeSpan.FromHours(1);
        Memory = 4;
        Queue = "normal";
        Project = string.Empty;
        EmailNotifications = false;
        EmailAddress = string.Empty;
    }

    /// <summary>
    /// Maximum walltime allowed for the job.
    /// </summary>
    public TimeSpan Walltime { get; init; }

	/// <summary>
	/// Amount of memory to be allocated to the job, in GiB.
	/// </summary>
	public uint Memory { get; init; }

	/// <summary>
	/// Queue to which the job should be submitted.
	/// </summary>
	public string Queue { get; init; }

	/// <summary>
	/// PBS project under which the job should be submitted.
	/// </summary>
	public string Project { get; init; }

	/// <summary>
	/// True to enable email notifications for the job, false otherwise.
	/// </summary>
	public bool EmailNotifications { get; init; }

	/// <summary>
	/// Email address to be used for the job. Only used if
	/// <see cref="EmailNotifications"/> is true.
	/// </summary>
	public string EmailAddress { get; init; }
}
