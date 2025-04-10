namespace LpjGuess.Runner.Models;

/// <summary>
/// A named set of parameters that can be applied to a run.
/// </summary>
public class ParameterSet
{
    /// <summary>
    /// Creates a new instance of ParameterSet.
    /// </summary>
    /// <param name="name">Name of this parameter set.</param>
    /// <param name="parameters">Dictionary of parameter names to their values.</param>
    public ParameterSet(string name, IReadOnlyDictionary<string, object[]> parameters)
    {
        Name = name;
        Parameters = parameters;
    }

    /// <summary>
    /// Name of this parameter set.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Dictionary of parameter names to their values.
    /// </summary>
    public IReadOnlyDictionary<string, object[]> Parameters { get; }
}
