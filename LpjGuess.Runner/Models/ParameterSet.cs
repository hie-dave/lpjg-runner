namespace LpjGuess.Runner.Models;

/// <summary>
/// A set of parameters and their values.
/// </summary>
public class ParameterSet
{
    /// <summary>
    /// Creates a new instance of ParameterSet.
    /// </summary>
    /// <param name="parameters">Parameters and their values in this set.</param>
    public ParameterSet(Dictionary<string, object[]> parameters)
    {
        Parameters = parameters;
    }

    /// <summary>
    /// Parameters and their values in this set.
    /// </summary>
    public Dictionary<string, object[]> Parameters { get; }
}
