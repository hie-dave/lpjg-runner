namespace LpjGuess.Runner.Models;

/// <summary>
/// Optional PBS configuration settings.
/// </summary>
public class PbsConfig
{
    /// <summary>
    /// Creates a new instance of PbsConfig.
    /// </summary>
    /// <param name="walltime">Maximum amount of walltime for each run.</param>
    /// <param name="memory">Amount of memory to be allocated to each run, in GiB.</param>
    /// <param name="queue">PBS queue to be used for running the jobs.</param>
    /// <param name="project">NCI project to be used for running the jobs.</param>
    /// <param name="emailNotifications">Whether to receive email notifications.</param>
    /// <param name="emailAddress">Email address for notifications.</param>
    public PbsConfig(
        string walltime,
        uint memory,
        string queue,
        string project,
        bool emailNotifications,
        string? emailAddress)
    {
        Walltime = walltime;
        Memory = memory;
        Queue = queue;
        Project = project;
        EmailNotifications = emailNotifications;
        EmailAddress = emailAddress;
    }

    /// <summary>
    /// Maximum amount of walltime for each run.
    /// </summary>
    public string Walltime { get; }

    /// <summary>
    /// Amount of memory to be allocated to each run, in GiB.
    /// </summary>
    public uint Memory { get; }

    /// <summary>
    /// PBS queue to be used for running the jobs.
    /// </summary>
    public string Queue { get; }

    /// <summary>
    /// NCI project to be used for running the jobs.
    /// </summary>
    public string Project { get; }

    /// <summary>
    /// Whether to receive email notifications of job status.
    /// </summary>
    public bool EmailNotifications { get; }

    /// <summary>
    /// Email address for notifications. Required if EmailNotifications is true.
    /// </summary>
    public string? EmailAddress { get; }
}
