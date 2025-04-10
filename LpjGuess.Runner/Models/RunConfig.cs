
namespace LpjGuess.Runner.Models;

/// <summary>
/// Configuration for an individual simulation run.
/// </summary>
public class RunConfig
{
    /// <summary>
    /// Creates a new instance of RunConfig.
    /// </summary>
    /// <param name="name">Name of this run configuration.</param>
    /// <param name="insFiles">List of instruction files to use for this run.</param>
    /// <param name="parameterSets">Names of parameter sets to apply to this run.</param>
    /// <param name="fullFactorial">Whether to use full factorial combinations.</param>
    public RunConfig(
        string name,
        string[] insFiles,
        string[] parameterSets,
        string[] pfts,
        bool fullFactorial)
    {
        Name = name;
        InsFiles = insFiles;
        ParameterSets = parameterSets;
        FullFactorial = fullFactorial;
        Pfts = pfts;
    }

    /// <summary>
    /// Name of this run configuration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// List of instruction files to use for this run.
    /// </summary>
    public string[] InsFiles { get; }

    /// <summary>
    /// Names of parameter sets to apply to this run.
    /// </summary>
    public string[] ParameterSets { get; }

    /// <summary>
    /// Names of PFTs to enable for this run. If not specified, all PFTs will be
    /// enabled. If specified, only these PFTs will be enabled.
    /// </summary>
    public string[] Pfts { get; }

    /// <summary>
    /// Whether to use full factorial combinations for this run.
    /// </summary>
    public bool FullFactorial { get; }

    /// <summary>
    /// Generate all factorials for this run configuration.
    /// </summary>
    /// <returns>A list of factorials.</returns>
    /// <exception cref="NotImplementedException"></exception>
    internal IEnumerable<Factorial> GenerateFactorials()
    {
        throw new NotImplementedException();
    }
}
