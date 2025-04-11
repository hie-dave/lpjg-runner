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
        // Extract the global configuration
        if (!model.TryGetValue("global", out var globalObj) || globalObj is not TomlTable globalTable)
            throw new ValidationException("Global configuration is required.");
        
        var global = globalTable.ToComplexType<GlobalConfig>();

        // Extract the PBS configuration (optional)
        PbsConfig? pbs = null;
        if (model.TryGetValue("pbs", out var pbsObj) && pbsObj is TomlTable pbsTable)
            pbs = pbsTable.ToComplexType<PbsConfig>();

        // Extract parameter sets
        IReadOnlyCollection<ParameterSet> parameterSets;
        if (model.TryGetValue("parameter_sets", out var setsObj))
        {
            if (setsObj is TomlTableArray setsArray)
            {
                parameterSets = setsArray.ToImmutableCollection<ParameterSet>();
            }
            else if (setsObj is TomlTable setsTable)
            {
                // Handle the [parameter_sets.name] format
                var parameterSetsList = new List<ParameterSet>();
                foreach (var kvp in setsTable)
                {
                    var parameters = new Dictionary<string, object[]>();
                    if (setsTable.TryGetValue(kvp.Key, out var setObj) && setObj is TomlTable setTable)
                    {
                        if (setTable.TryGetValue("parameters", out var paramsObj) && paramsObj is TomlTable paramsTable)
                        {
                            foreach (var paramKvp in paramsTable)
                            {
                                if (paramKvp.Value is TomlArray array)
                                {
                                    parameters[paramKvp.Key] = array.Cast<object>().ToArray();
                                }
                            }
                        }
                    }
                    
                    parameterSetsList.Add(new ParameterSet(kvp.Key, parameters));
                }
                parameterSets = parameterSetsList;
            }
            else
            {
                throw new ValidationException("Parameter sets dictionary is required.");
            }
        }
        else
        {
            throw new ValidationException("Parameter sets dictionary is required.");
        }

        // Extract runs
        RunConfig[] runs;
        if (model.TryGetValue("runs", out var runsObj) && runsObj is TomlTableArray runsArray)
        {
            runs = runsArray.Select(r => r.ToComplexType<RunConfig>()).ToArray();
        }
        else
        {
            throw new ValidationException("Runs list is required.");
        }

        // Validate the configuration
        ValidateConfiguration(global, pbs, parameterSets, runs);

        // Create the configuration
        return new Configuration(global, pbs, parameterSets, runs);
    }

    private void ValidateConfiguration(GlobalConfig global, PbsConfig? pbs, IReadOnlyCollection<ParameterSet> parameterSets, RunConfig[] runs)
    {
        // Validate global config
        if (string.IsNullOrWhiteSpace(global.GuessPath))
            throw new ValidationException("GuessPath is required.");
        if (string.IsNullOrWhiteSpace(global.InputModule))
            throw new ValidationException("InputModule is required.");
        if (string.IsNullOrWhiteSpace(global.OutputDirectory))
            throw new ValidationException("OutputDirectory is required.");
        if (global.CpuCount == 0)
            throw new ValidationException("CpuCount must be greater than 0.");

        // Validate PBS config if present
        if (pbs != null)
        {
            if (string.IsNullOrWhiteSpace(pbs.Walltime))
                throw new ValidationException("Walltime is required when PBS is configured.");
            if (string.IsNullOrWhiteSpace(pbs.Queue))
                throw new ValidationException("Queue is required when PBS is configured.");
            if (string.IsNullOrWhiteSpace(pbs.Project))
                throw new ValidationException("Project is required when PBS is configured.");
            if (pbs.Memory == 0)
                throw new ValidationException("Memory must be greater than 0.");
            if (pbs.EmailNotifications && string.IsNullOrWhiteSpace(pbs.EmailAddress))
                throw new ValidationException("EmailAddress is required when EmailNotifications is true.");
        }

        // Validate parameter sets
        foreach (var set in parameterSets)
        {
            if (string.IsNullOrWhiteSpace(set.Name))
                throw new ValidationException("Parameter set names cannot be empty.");
            if (set.Parameters == null)
                throw new ValidationException($"Parameters dictionary is required in set '{set.Name}'.");

            foreach (var param in set.Parameters)
            {
                if (string.IsNullOrWhiteSpace(param.Key))
                    throw new ValidationException($"Parameter names cannot be empty in set '{set.Name}'.");
                if (param.Value == null || param.Value.Length == 0)
                    throw new ValidationException($"Parameter '{param.Key}' in set '{set.Name}' must have values.");
            }
        }

        // Validate runs
        var validSetNames = parameterSets.Select(p => p.Name).ToHashSet();
        foreach (var run in runs)
        {
            if (string.IsNullOrWhiteSpace(run.Name))
                throw new ValidationException("Run name is required.");
            if (run.InsFiles == null || run.InsFiles.Length == 0)
                throw new ValidationException($"InsFiles are required in run '{run.Name}'.");
            if (run.ParameterSets == null || run.ParameterSets.Length == 0)
                throw new ValidationException($"ParameterSets are required in run '{run.Name}'.");

            foreach (string set in run.ParameterSets)
            {
                if (!validSetNames.Contains(set))
                    throw new ValidationException($"Run '{run.Name}' references undefined parameter set '{set}'.");
            }
        }
    }
}
