namespace LpjGuess.Runner.Models.Dto;

/// <summary>
/// Data transfer object for GlobalConfig.
/// </summary>
public class GlobalConfigDto
{
    public string? GuessPath { get; set; }
    public string? InputModule { get; set; }
    public string? OutputDirectory { get; set; }
    public ushort CpuCount { get; set; }
    public bool DryRun { get; set; }
    public bool Parallel { get; set; }
}
