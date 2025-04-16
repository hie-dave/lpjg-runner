using LpjGuess.Runner.Models.Validation;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Root configuration for running LPJ-Guess simulations.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Creates a new instance of Configuration.
    /// </summary>
    /// <param name="global">Global configuration settings.</param>
    /// <param name="pbs">Optional PBS configuration settings.</param>
    /// <param name="parameterSets">Parameter sets that can be referenced by runs.</param>
    /// <param name="runs">Individual run configurations.</param>
    public Configuration(
        GlobalConfig global,
        PbsConfig? pbs,
        IReadOnlyCollection<ParameterSet> parameterSets,
        RunConfig[] runs)
    {
        Global = global;
        Pbs = pbs;
        ParameterSets = parameterSets;
        Runs = runs;
    }

    /// <summary>
    /// Global configuration settings.
    /// </summary>
    public GlobalConfig Global { get; }

    /// <summary>
    /// Optional PBS configuration settings.
    /// </summary>
    public PbsConfig? Pbs { get; }

    /// <summary>
    /// Parameter sets that can be referenced by runs.
    /// </summary>
    public IReadOnlyCollection<ParameterSet> ParameterSets { get; }

    /// <summary>
    /// Individual run configurations.
    /// </summary>
    public RunConfig[] Runs { get; }

    public void Validate()
    {
        // Validate global config
        if (string.IsNullOrWhiteSpace(Global.GuessPath))
            throw new ValidationException("GuessPath is required.");
        if (string.IsNullOrWhiteSpace(Global.InputModule))
            throw new ValidationException("InputModule is required.");
        if (string.IsNullOrWhiteSpace(Global.OutputDirectory))
            throw new ValidationException("OutputDirectory is required.");
        if (Global.CpuCount == 0)
            throw new ValidationException("CpuCount must be greater than 0.");

        // Validate PBS config if present
        if (Pbs != null)
        {
            if (string.IsNullOrWhiteSpace(Pbs.Walltime))
                throw new ValidationException("Walltime is required when PBS is configured.");
            if (string.IsNullOrWhiteSpace(Pbs.Queue))
                throw new ValidationException("Queue is required when PBS is configured.");
            if (string.IsNullOrWhiteSpace(Pbs.Project))
                throw new ValidationException("Project is required when PBS is configured.");
            if (Pbs.Memory == 0)
                throw new ValidationException("Memory must be greater than 0.");
            if (Pbs.EmailNotifications && string.IsNullOrWhiteSpace(Pbs.EmailAddress))
                throw new ValidationException("EmailAddress is required when EmailNotifications is true.");
        }

        // Validate parameter sets
        foreach (ParameterSet set in ParameterSets)
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
        HashSet<string> validSetNames = ParameterSets.Select(p => p.Name).ToHashSet();
        foreach (RunConfig run in Runs)
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
