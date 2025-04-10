using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Validation;
using LpjGuess.Runner.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using Tomlyn;
using Xunit;

namespace LpjGuess.Runner.Tests.Parsers;

/// <summary>
/// Tests for TomlParser.
/// </summary>
public class TomlParserTests : IDisposable
{
    private readonly TomlParser parser;
    private readonly List<string> tempFiles;

    /// <summary>
    /// Creates a new instance of TomlParserTests.
    /// </summary>
    public TomlParserTests()
    {
        parser = new TomlParser();
        tempFiles = new List<string>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (string file in tempFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }

    [Fact]
    public void Parse_ValidConfig_ReturnsConfiguration()
    {
        const string toml = """
            [global]
            guess_path = "/path/to/guess"
            input_module = "nc"
            output_directory = "/path/to/output"
            cpu_count = 1

            [pbs]
            walltime = "01:00:00"
            memory = 1
            queue = "normal"
            project = "pt17"
            email_notifications = true
            email_address = "user@example.com"

            [[parameter_sets]]
            name = "baseline"
            co2 = [350, 400, 450]

            [[runs]]
            name = "baseline_run"
            ins_files = ["/path/to/first.ins"]
            parameter_sets = ["baseline"]
            full_factorial = true
            """;

        Configuration config = parser.Parse(CreateTempFile(toml));

        Assert.NotNull(config);
        Assert.NotNull(config.Global);
        Assert.Equal("/path/to/guess", config.Global.GuessPath);
        Assert.Equal("nc", config.Global.InputModule);
        Assert.Equal("/path/to/output", config.Global.OutputDirectory);
        Assert.Equal((ushort)1, config.Global.CpuCount);

        Assert.NotNull(config.Pbs);
        Assert.Equal("01:00:00", config.Pbs.Walltime);
        Assert.Equal(1u, config.Pbs.Memory);
        Assert.Equal("normal", config.Pbs.Queue);
        Assert.Equal("pt17", config.Pbs.Project);
        Assert.True(config.Pbs.EmailNotifications);
        Assert.Equal("user@example.com", config.Pbs.EmailAddress);

        Assert.NotNull(config.ParameterSets);
        Assert.Single(config.ParameterSets);
        Assert.Contains(config.ParameterSets, p => p.Name == "baseline");
        ParameterSet baseline = config.ParameterSets.First(p => p.Name == "baseline");
        Assert.NotNull(baseline.Parameters);
        Assert.Single(baseline.Parameters);
        Assert.True(baseline.Parameters.ContainsKey("co2"));
        Assert.Equal(3, baseline.Parameters["co2"].Length);

        Assert.NotNull(config.Runs);
        Assert.Single(config.Runs);
        Assert.Equal("baseline_run", config.Runs[0].Name);
        Assert.Single(config.Runs[0].InsFiles);
        Assert.Equal("/path/to/first.ins", config.Runs[0].InsFiles[0]);
        Assert.Single(config.Runs[0].ParameterSets);
        Assert.Equal("baseline", config.Runs[0].ParameterSets[0]);
        Assert.True(config.Runs[0].FullFactorial);
    }

    [Fact]
    public void Parse_MissingGlobal_ThrowsValidationException()
    {
        const string toml = """
            [[runs]]
            name = "test"
            ins_files = ["test.ins"]
            parameter_sets = []
            full_factorial = true
            """;

        ParserException ex = Assert.Throws<ParserException>(
            () => parser.Parse(CreateTempFile(toml)));
        ValidationException innerException = Assert.IsType<ValidationException>(ex.InnerException);
        Assert.Equal("Global configuration is required.", innerException.Message);
    }

    [Fact]
    public void Parse_MissingRequiredPbsField_ThrowsValidationException()
    {
        const string toml = """
            [global]
            guess_path = "/path/to/guess"
            input_module = "nc"
            output_directory = "/path/to/output"
            cpu_count = 1

            [pbs]
            memory = 1
            queue = "normal"
            project = "pt17"
            email_notifications = true
            email_address = "user@example.com"

            [parameter_sets.baseline]
            parameters = { co2 = [350, 400, 450] }

            [[runs]]
            name = "test"
            ins_files = ["test.ins"]
            parameter_sets = []
            full_factorial = true
            """;

        ParserException ex = Assert.Throws<ParserException>(
            () => parser.Parse(CreateTempFile(toml)));
        ValidationException innerException = Assert.IsType<ValidationException>(ex.InnerException);
        Assert.Equal("Walltime is required when PBS is configured.", innerException.Message);
    }

    [Fact]
    public void Parse_InvalidParameterSetReference_ThrowsValidationException()
    {
        const string toml = """
            [global]
            guess_path = "/path/to/guess"
            input_module = "nc"
            output_directory = "/path/to/output"
            cpu_count = 1

            [parameter_sets.baseline]
            parameters = { co2 = [350, 400, 450] }

            [[runs]]
            name = "test"
            ins_files = ["test.ins"]
            parameter_sets = ["missing"]
            full_factorial = true
            """;

        ParserException ex = Assert.Throws<ParserException>(
            () => parser.Parse(CreateTempFile(toml)));
        ValidationException innerException = Assert.IsType<ValidationException>(ex.InnerException);
        Assert.Equal("Run 'test' references undefined parameter set 'missing'.", innerException.Message);
    }

    private string CreateTempFile(string content)
    {
        string path = Path.GetTempFileName();
        tempFiles.Add(path);
        File.WriteAllText(path, content);
        return path;
    }
}
