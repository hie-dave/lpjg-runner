using LpjGuess.Runner.Extensions;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Validation;
using Tomlyn;
using Tomlyn.Model;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// This class can parse a .toml input file.
/// </summary>
internal class TomlParser : IParser
{
    /// <inheritdoc />
    public Configuration Parse(string file)
    {
        try
        {
            string content = File.ReadAllText(file);
            var model = Toml.ToModel(content);
            return ParseConfiguration(model);
        }
        catch (Exception error)
        {
            throw new ParserException(file, error);
        }
    }

    private Configuration ParseConfiguration(TomlTable model)
    {
        if (!model.TryConvert(out Configuration? config))
            throw new InvalidOperationException($"Failed to parse configuration settings from toml");
        config.Validate();
        return config;
    }
}
