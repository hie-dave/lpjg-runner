namespace LpjGuess.Runner.Models.Dto;

/// <summary>
/// Data transfer object for PbsConfig.
/// </summary>
public class PbsConfigDto
{
    public string? Walltime { get; set; }
    public uint Memory { get; set; }
    public string? Queue { get; set; }
    public string? Project { get; set; }
    public bool EmailNotifications { get; set; }
    public string? EmailAddress { get; set; }
}
