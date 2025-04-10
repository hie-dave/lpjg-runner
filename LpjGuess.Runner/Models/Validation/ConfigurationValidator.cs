using LpjGuess.Runner.Models.Dto;

namespace LpjGuess.Runner.Models.Validation;

/// <summary>
/// Validates configuration DTOs before mapping to immutable types.
/// </summary>
public class ConfigurationValidator
{
    /// <summary>
    /// Validates the configuration DTO.
    /// </summary>
    /// <param name="dto">The configuration DTO to validate.</param>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public void Validate(ConfigurationDto dto)
    {
        if (dto.Global == null)
            throw new ValidationException("Global configuration is required.");
        if (dto.ParameterSets == null)
            throw new ValidationException("Parameter sets dictionary is required.");
        if (dto.Runs == null)
            throw new ValidationException("Runs list is required.");

        ValidateGlobal(dto.Global);
        ValidatePbs(dto.Pbs);
        ValidateParameterSets(dto.ParameterSets);
        ValidateRuns(dto.Runs, dto.ParameterSets.Keys);
    }

    private void ValidateGlobal(GlobalConfigDto global)
    {
        if (string.IsNullOrWhiteSpace(global.GuessPath))
            throw new ValidationException("GuessPath is required.");
        if (string.IsNullOrWhiteSpace(global.InputModule))
            throw new ValidationException("InputModule is required.");
        if (string.IsNullOrWhiteSpace(global.OutputDirectory))
            throw new ValidationException("OutputDirectory is required.");
        if (global.CpuCount == 0)
            throw new ValidationException("CpuCount must be greater than 0.");
    }

    private void ValidatePbs(PbsConfigDto? pbs)
    {
        if (pbs == null)
            return;

        if (string.IsNullOrWhiteSpace(pbs.Walltime))
            throw new ValidationException("Walltime is required when PBS is configured.");
        if (string.IsNullOrWhiteSpace(pbs.Queue))
            throw new ValidationException("Queue is required when PBS is configured.");
        if (string.IsNullOrWhiteSpace(pbs.Project))
            throw new ValidationException("Project is required when PBS is configured.");
        if (pbs.Memory == 0)
            throw new ValidationException("Memory must be greater than 0.");
        if (pbs.EmailNotifications && string.IsNullOrWhiteSpace(pbs.EmailAddress))
            throw new ValidationException(
                "EmailAddress is required when EmailNotifications is true.");
    }

    private void ValidateParameterSets(Dictionary<string, ParameterSetDto> sets)
    {
        foreach (var kvp in sets)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
                throw new ValidationException("Parameter set names cannot be empty.");
            if (kvp.Value.Parameters == null)
                throw new ValidationException(
                    $"Parameters dictionary is required in set '{kvp.Key}'.");

            foreach (var param in kvp.Value.Parameters)
            {
                if (string.IsNullOrWhiteSpace(param.Key))
                    throw new ValidationException(
                        $"Parameter names cannot be empty in set '{kvp.Key}'.");
                if (param.Value == null || param.Value.Length == 0)
                    throw new ValidationException(
                        $"Parameter '{param.Key}' in set '{kvp.Key}' must have values.");
            }
        }
    }

    private void ValidateRuns(List<RunConfigDto> runs, ICollection<string> validSets)
    {
        foreach (RunConfigDto run in runs)
        {
            if (string.IsNullOrWhiteSpace(run.Name))
                throw new ValidationException("Run name is required.");
            if (run.InsFiles == null || run.InsFiles.Length == 0)
                throw new ValidationException($"InsFiles are required in run '{run.Name}'.");
            if (run.ParameterSets == null || run.ParameterSets.Length == 0)
                throw new ValidationException(
                    $"ParameterSets are required in run '{run.Name}'.");

            foreach (string set in run.ParameterSets)
            {
                if (!validSets.Contains(set))
                    throw new ValidationException(
                        $"Run '{run.Name}' references undefined parameter set '{set}'.");
            }
        }
    }
}
