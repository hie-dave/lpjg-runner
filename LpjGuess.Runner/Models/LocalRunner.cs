using System.Diagnostics;
using System.Text;
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
    /// Standard output and error generated by the child process.
    /// </summary>
    private readonly StringBuilder output;

    /// <summary>
    /// Create a new <see cref="LocalRunner"/> instance.
    /// </summary>
    /// <param name="settings">Configuration parameters controlling how the job should be run.</param>
    public LocalRunner(RunSettings settings)
    {
        if (Environment.ProcessorCount > 64 && settings.CpuCount > 64)
            throw new NotImplementedException("TODO: use platform-specific API to suppost >64 CPUs");
        if (settings.CpuCount > Environment.ProcessorCount)
            throw new NotImplementedException($"cpu_count must be < NCPUs ({Environment.ProcessorCount} in this case), but is: {settings.CpuCount}");
        this.settings = settings;
        output = new StringBuilder();
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

        using Process proc = new Process();
        proc.StartInfo.FileName = settings.GuessPath;
        proc.StartInfo.ArgumentList.Add("-input");
        proc.StartInfo.ArgumentList.Add(settings.InputModule);
        proc.StartInfo.ArgumentList.Add(job.InsFile);
        proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(job.InsFile);
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;

        proc.Start();

        using CpuAffinity cpu = CpuAffinity.Acquire();
        cpu.SetAffinity(proc);

        // Start output parsing task
        Task stdoutTask = ParseOutputAsync(proc.StandardOutput, job.Name, ct);
        Task stderrTask = ParseErrorAsync(proc.StandardError, ct);
        
        // Wait for process completion
        await proc.WaitForExitAsync(ct);
        await Task.WhenAll(stdoutTask, stderrTask);

        if (proc.ExitCode != 0 && !ct.IsCancellationRequested)
            throw new ModelException($"{job.Name}: execution failed with exit code: {proc.ExitCode}", output.ToString());

        if (ct.IsCancellationRequested && !proc.HasExited)
            proc.Kill();
        ct.ThrowIfCancellationRequested();
    }

    private async Task ParseErrorAsync(StreamReader errorStream, CancellationToken ct)
    {
        string? line;
        while ((line = await errorStream.ReadLineAsync()) != null &&
               !ct.IsCancellationRequested)
            output.AppendLine(line);
    }

    private async Task ParseOutputAsync(StreamReader outputStream, string jobName, CancellationToken ct)
    {
        string? line;
        while ((line = await outputStream.ReadLineAsync()) != null &&
                !ct.IsCancellationRequested)
        {
            if (string.IsNullOrEmpty(line))
                continue;
            
            // Parse progress from output (example format: "Progress: 50%")
            Match match = Regex.Match(line, @"([0-9]+)%[ \t]complete,[ \t]+([0-9]+:[0-9]+:[0-9]+)[ \t]+elapsed,[ \t]+([0-9]+:[0-9]+:[0-9]+)[ \t]remaining");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int percentage))
                ReportProgress(jobName, percentage);
            else
            {
                output.AppendLine(line);
                if (line.Contains("Finished"))
                    ReportProgress(jobName, 100);
            }
        }
    }

    private void ReportProgress(string jobName, int percentage)
    {
        ProgressChanged?.Invoke(this, new ProgressEventArgs
        {
            Percentage = percentage,
            JobName = jobName,
        });
    }
}
