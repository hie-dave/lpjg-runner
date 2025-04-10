namespace LpjGuess.Runner.Models.Dto;

/// <summary>
/// Data transfer object for Configuration.
/// </summary>
public class ConfigurationDto
{
    public GlobalConfigDto? Global { get; set; }
    public PbsConfigDto? Pbs { get; set; }
    public List<ParameterSetDto>? ParameterSets { get; set; }
    public List<RunConfigDto>? Runs { get; set; }
}
