namespace LpjGuess.Runner.Models;

/// <summary>
/// Global configuration settings that apply across all runs.
/// </summary>
public class GlobalConfig 
{
    /// <summary>
    /// Creates a new instance of GlobalConfig.
    /// </summary>
    /// <param name="guessPath">Path to the LPJ-Guess executable.</param>
    /// <param name="inputModule">Input module to be used by LPJ-Guess.</param>
    /// <param name="outputDirectory">Output directory for all runs.</param>
    /// <param name="cpuCount">Number of CPUs to be allocated to each run.</param>
    /// <param name="dryRun">If true, do a dry run of the job manager, printing out the commands
    /// instead of running them.</param>
    public GlobalConfig(
        string guessPath,
        string inputModule,
        string outputDirectory,
        ushort cpuCount,
        bool dryRun)
    {
        GuessPath = guessPath;
        InputModule = inputModule;
        OutputDirectory = outputDirectory;
        CpuCount = cpuCount;
        DryRun = dryRun;
    }

    /// <summary>
    /// Path to the LPJ-Guess executable.
    /// </summary>
    public string GuessPath { get; }

    /// <summary>
    /// Input module to be used by LPJ-Guess.
    /// </summary>
    public string InputModule { get; }

    /// <summary>
    /// Output directory for all runs.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Number of CPUs to be allocated to each run.
    /// </summary>
    public ushort CpuCount { get; }

    /// <summary>
    /// If true, do a dry run of the job manager, printing out the commands
    /// instead of running them.
    /// </summary>
    public bool DryRun { get; }

    /// <summary>
    /// Iff true, jobs will be generated in parallel. This has no effect on job
    /// execution.
    /// </summary>
    public bool Parallel { get; }
}
