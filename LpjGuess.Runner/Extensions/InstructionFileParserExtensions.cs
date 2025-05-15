using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Runner.Extensions;

/// <summary>
/// Extension methods for <see cref="InstructionFileParser"/> 
/// </summary>
public static class InstructionFileParserExtensions
{
    /// <summary>
    /// Name of the parameter that specifies the gridlist file.
    /// </summary>
    private const string paramGridlist = "file_gridlist";

    /// <summary>
    /// Name of the parameter that specifies the gridlist file when using the
    /// CF input module.
    /// </summary>
    private const string paramGridlistCf = "file_gridlist_cf";

    /// <summary>
    /// Name of the "str" parameter".
    /// </summary>
    private const string strBlock = "str";

    /// <summary>
    /// Name of the block 
    /// </summary>
    private const string parameterBlock = "param";

    /// <summary>
    /// Get the path to the gridlist, as it appears in the instruction file.
    /// </summary>
    /// <param name="parser">An instruction file parser.</param>
    /// <returns>The path (possibly relative) to the gridlist file.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the instruction file doesn't contain a gridlist parameter.</exception>
    private static string GetGridlistRelative(this InstructionFileParser parser)
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(paramGridlist);
        if (parameter != null)
            return parameter.AsString();
        parameter = parser.GetBlockParameter(parameterBlock, paramGridlistCf, strBlock);
        if (parameter != null)
            return parameter.AsString();
        throw new InvalidOperationException($"Instruction file {parser.FilePath} does not contain a gridlist parameter");
    }

    /// <summary>
    /// Get the gridlist parameter from an instruction file.
    /// </summary>
    /// <param name="parser">The instruction file parser.</param>
    /// <returns>Absolute path to the gridlist file.</returns>
    public static string GetGridlist(this InstructionFileParser parser)
    {
        string relativePath = parser.GetGridlistRelative();
        string? directory = Path.GetDirectoryName(parser.FilePath);
        if (directory == null)
            // Directory will be null if path is root directory (cannot happen),
            // or if the path doesn't contain a directory component, in which
            // case, we can assume that it is in the current directory.
            return relativePath;

        string fullPath = Path.GetFullPath(Path.Combine(directory, relativePath));
        return fullPath;
    }
}
