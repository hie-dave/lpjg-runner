
namespace LpjGuess.Runner.Models;

public class JobManager
{
    /// <summary>
    /// Max number of CPUs to use to run the jobs.
    /// </summary>
    private readonly uint cpuCount;

    /// <summary>
    /// List of running tasks.
    /// </summary>
    private readonly List<Task> runningTasks;

    /// <summary>
    /// Create a new <see cref="JobManager"/> instance.
    /// </summary>
    /// <param name="cpuCount">Max number of CPUs to use to run the jobs.</param>
    public JobManager(uint cpuCount)
    {
        this.cpuCount = cpuCount;
        runningTasks = new List<Task>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jobs">Jobs to be run.</param>
    public async Task Run(IEnumerable<IJob> jobs, CancellationToken ct)
    {
        foreach (IJob job in jobs)
        {
            ct.ThrowIfCancellationRequested();
            if (runningTasks.Count >= cpuCount)
            {
                int index = Task.WaitAny(runningTasks.ToArray());
                runningTasks.RemoveAt(index);
            }
            runningTasks.Add(job.Run(ct));
        }
    }
}
