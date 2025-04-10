namespace LpjGuess.Runner.Models.Validation;

/// <summary>
/// Exception thrown when configuration validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Creates a new instance of ValidationException.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public ValidationException(string message) : base(message)
    {
    }
}
