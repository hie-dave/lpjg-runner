namespace LpjGuess.Runner.Models;

/// <summary>
/// An interface to a class which can run the model.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Run the model on the specified instruction file.
    /// </summary>
    /// <param name="insFile">Instruction file to be run.</param>
    /// <param name="ct">Cancellation token.</param>
    Task Run(CancellationToken ct);
}
