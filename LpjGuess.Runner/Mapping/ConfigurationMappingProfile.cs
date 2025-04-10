using AutoMapper;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Dto;

namespace LpjGuess.Runner.Mapping;

/// <summary>
/// AutoMapper profile for configuration types.
/// </summary>
public class ConfigurationMappingProfile : Profile
{
    /// <summary>
    /// Creates a new instance of ConfigurationMappingProfile.
    /// </summary>
    public ConfigurationMappingProfile()
    {
        CreateMap<ConfigurationDto, Configuration>();
        CreateMap<GlobalConfigDto, GlobalConfig>();
        CreateMap<PbsConfigDto, PbsConfig>();
        CreateMap<ParameterSetDto, ParameterSet>();
        CreateMap<RunConfigDto, RunConfig>();
    }
}
