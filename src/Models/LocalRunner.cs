
using System.Diagnostics;

namespace LpjGuess.Runner.Models;

/// <summary>
/// A class which runs the model in the local environment (ie without PBS).
/// </summary>
public class LocalRunner : IRunner
{
    /// <summary>
    /// Path to the model executable.
    /// </summary>
    private readonly RunSettings settings;

    /// <summary>
    /// Create a new <see cref="LocalRunner"/> instance.
    /// </summary>
    /// <param name="executable">Path to the model executable.</param>
    public LocalRunner(RunSettings settings)
    {
        this.settings = settings;
    }

    /// <inheritdoc /> 
    public async Task Run(string insFile, CancellationToken ct)
    {
        if (settings.DryRun)
            return;

        using (Process proc = new Process())
        {
            proc.StartInfo.FileName = settings.GuessPath;
            proc.StartInfo.ArgumentList.Add("-input");
            proc.StartInfo.ArgumentList.Add(settings.InputModule);
            proc.StartInfo.ArgumentList.Add(insFile);
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(insFile);

            // Run the submit script.
            proc.Start();
            await proc.WaitForExitAsync(ct);
            if (ct.IsCancellationRequested && !proc.HasExited)
                proc.Kill();
            ct.ThrowIfCancellationRequested();
        }
    }
}
