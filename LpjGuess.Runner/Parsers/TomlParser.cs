using AutoMapper;
using LpjGuess.Runner.Mapping;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Dto;
using LpjGuess.Runner.Models.Validation;
using Tomlyn;

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
            ConfigurationDto dto = Toml.ToModel<ConfigurationDto>(content);
            validator.Validate(dto);
            return mapper.Map<Configuration>(dto);
        }
        catch (Exception error)
        {
            throw new ParserException(file, error);
        }
    }
}
