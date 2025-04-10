namespace LpjGuess.Runner.Models.Dto;

/// <summary>
/// Data transfer object for RunConfig.
/// </summary>
public class RunConfigDto
{
    public string? Name { get; set; }
    public string[]? InsFiles { get; set; }
    public string[]? ParameterSets { get; set; }
    public string[]? Pfts { get; set; }
    public bool FullFactorial { get; set; }
}
