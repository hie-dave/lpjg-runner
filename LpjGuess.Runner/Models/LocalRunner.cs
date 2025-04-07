using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

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
        if (Environment.ProcessorCount > 64 && settings.CpuCount > 64)
            throw new NotImplementedException("TODO: use platform-specific API to suppost >64 CPUs");
        if (settings.CpuCount > Environment.ProcessorCount)
            throw new NotImplementedException($"cpu_count must be < NCPUs ({Environment.ProcessorCount} in this case), but is: {settings.CpuCount}");
        this.settings = settings;
    }

    /// <summary>
    /// Event raised when the progress of the job changes.
    /// </summary>
    public event EventHandler<ProgressEventArgs>? ProgressChanged;

    /// <inheritdoc /> 
    public async Task RunAsync(Job job, CancellationToken ct)
    {
        if (settings.DryRun)
            return;

        using (Process proc = new Process())
        {
            proc.StartInfo.FileName = settings.GuessPath;
            proc.StartInfo.ArgumentList.Add("-input");
            proc.StartInfo.ArgumentList.Add(settings.InputModule);
            proc.StartInfo.ArgumentList.Add(job.InsFile);
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(job.InsFile);
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();

            using CpuAffinity cpu = CpuAffinity.Acquire();
            cpu.SetAffinity(proc);

            // Start output parsing task
            var parseTask = ParseOutputAsync(proc, job.Name, ct);
            
            // Wait for process completion
            await proc.WaitForExitAsync(ct);
            await parseTask;

            if (ct.IsCancellationRequested && !proc.HasExited)
                proc.Kill();
            ct.ThrowIfCancellationRequested();
        }
    }

    private async Task ParseOutputAsync(Process proc, string jobName, CancellationToken ct)
    {
        while (!proc.HasExited && !ct.IsCancellationRequested)
        {
            string? line = await proc.StandardOutput.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                continue;
            
            // Parse progress from output (example format: "Progress: 50%")
            Match match = Regex.Match(line, @"([0-9]+)%[ \t]complete,[ \t]+([0-9]+:[0-9]+:[0-9]+)[ \t]+elapsed,[ \t]+([0-9]+:[0-9]+:[0-9]+)[ \t]remaining");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int percentage))
            {
                ProgressChanged?.Invoke(this, new ProgressEventArgs
                {
                    Percentage = percentage,
                    JobName = jobName,
                });
            }
            else if (line.Contains("Finished"))
            {
                ProgressChanged?.Invoke(this, new ProgressEventArgs
                {
                    Percentage = 100,
                    JobName = jobName,
                });
            }
        }
    }
}
