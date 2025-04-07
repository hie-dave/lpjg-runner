namespace LpjGuess.Runner.Models;

/// <summary>
/// Interface for running LPJ-Guess simulations.
/// </summary>
public interface IRunner
{
	/// <summary>
	/// Event raised when progress is reported.
	/// </summary>
	event EventHandler<ProgressEventArgs> ProgressChanged;

	/// <summary>
	/// Run the simulation.
	/// </summary>
	/// <param name="job">The job to be run.</param>
	/// <param name="ct">Cancellation token.</param>
	Task RunAsync(Job job, CancellationToken ct);
}

/// <summary>
/// Progress event arguments.
/// </summary>
public class ProgressEventArgs : EventArgs
{
	/// <summary>
	/// Current progress percentage (0-100).
	/// </summary>
	public int Percentage { get; init; }

	/// <summary>
	/// Job name.
	/// </summary>
	public string JobName { get; init; } = string.Empty;
}
