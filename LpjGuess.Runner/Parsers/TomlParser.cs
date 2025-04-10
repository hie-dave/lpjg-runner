using AutoMapper;
using LpjGuess.Runner.Mapping;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Dto;
using LpjGuess.Runner.Models.Validation;
using Tomlyn;
using Tomlyn.Model;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// This class can parse a .toml input file.
/// </summary>
internal class TomlParser : IParser
{
    private readonly IMapper mapper;
    private readonly ConfigurationValidator validator;

    /// <summary>
    /// Creates a new instance of TomlParser.
    /// </summary>
    public TomlParser()
    {
        MapperConfiguration config = new(cfg => cfg.AddProfile<ConfigurationMappingProfile>());
        mapper = config.CreateMapper();
        validator = new ConfigurationValidator();
    }

    /// <inheritdoc />
    public Configuration Parse(string file)
    {
        try
        {
            string content = File.ReadAllText(file);
            var model = Toml.ToModel(content);
            ConfigurationDto dto = ParseConfiguration(model);
            validator.Validate(dto);
            return mapper.Map<Configuration>(dto);
        }
        catch (Exception error)
        {
            throw new ParserException(file, error);
        }
    }

    private ConfigurationDto ParseConfiguration(TomlTable model)
    {
        var dto = new ConfigurationDto();

        if (model.TryGetValue("global", out var globalObj) && globalObj is TomlTable global)
            dto.Global = mapper.Map<GlobalConfigDto>(global);

        if (model.TryGetValue("pbs", out var pbsObj) && pbsObj is TomlTable pbs)
            dto.Pbs = mapper.Map<PbsConfigDto>(pbs);

        if (model.TryGetValue("parameter_sets", out var setsObj) && setsObj is TomlArray sets)
        {
            dto.ParameterSets = new List<ParameterSetDto>();
            foreach (var setObj in sets)
            {
                if (setObj is TomlTable set)
                {
                    var paramSet = new ParameterSetDto { Parameters = new() };
                    foreach (var kvp in set)
                    {
                        if (kvp.Key == "name")
                            paramSet.Name = kvp.Value as string;
                        else if (kvp.Value is TomlArray array)
                            paramSet.Parameters[kvp.Key] = array.Select(v => v).ToArray();
                    }
                    dto.ParameterSets.Add(paramSet);
                }
            }
        }

        if (model.TryGetValue("runs", out var runsObj) && runsObj is TomlArray runs)
            dto.Runs = runs.Select(r => mapper.Map<RunConfigDto>(r)).ToList();

        return dto;
    }
}
