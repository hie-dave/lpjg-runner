namespace LpjGuess.Runner.Models.Dto;

/// <summary>
/// Data transfer object for ParameterSet.
/// </summary>
public class ParameterSetDto
{
    /// <summary>
    /// Name of the parameter set.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Dictionary of parameter names to their values.
    /// </summary>
    public Dictionary<string, object[]> Parameters { get; set; } = new();
}
