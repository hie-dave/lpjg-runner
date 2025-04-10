namespace LpjGuess.Runner.Models;

/// <summary>
/// Root configuration for running LPJ-Guess simulations.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Creates a new instance of Configuration.
    /// </summary>
    /// <param name="global">Global configuration settings.</param>
    /// <param name="pbs">Optional PBS configuration settings.</param>
    /// <param name="parameterSets">Parameter sets that can be referenced by runs.</param>
    /// <param name="runs">Individual run configurations.</param>
    public Configuration(
        GlobalConfig global,
        PbsConfig? pbs,
        IReadOnlyCollection<ParameterSet> parameterSets,
        RunConfig[] runs)
    {
        Global = global;
        Pbs = pbs;
        ParameterSets = parameterSets;
        Runs = runs;
    }

    /// <summary>
    /// Global configuration settings.
    /// </summary>
    public GlobalConfig Global { get; }

    /// <summary>
    /// Optional PBS configuration settings.
    /// </summary>
    public PbsConfig? Pbs { get; }

    /// <summary>
    /// Parameter sets that can be referenced by runs.
    /// </summary>
    public IReadOnlyCollection<ParameterSet> ParameterSets { get; }

    /// <summary>
    /// Individual run configurations.
    /// </summary>
    public RunConfig[] Runs { get; }
}
