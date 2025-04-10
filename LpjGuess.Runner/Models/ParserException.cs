namespace LpjGuess.Runner.Models;

/// <summary>
/// Exception thrown when configuration parsing fails.
/// </summary>
public class ParserException : Exception
{
    /// <summary>
    /// Creates a new instance of ParserException.
    /// </summary>
    /// <param name="fileName">The name of the file that could not be parsed.</param>
    /// <param name="innerException">The inner exception that caused the parsing failure.</param>
    public ParserException(string fileName, Exception innerException) : base($"Unable to parse input file '{fileName}'", innerException)
    {
    }
}
