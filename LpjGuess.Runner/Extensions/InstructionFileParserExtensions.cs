using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Runner.Extensions;

/// <summary>
/// Extension methods for <see cref="InstructionFileParser"/> 
/// </summary>
public static class InstructionFileParserExtensions
{
    /// <summary>
    /// Name of the instruction file parameter specifying the output directory.
    /// </summary>
    private const string outputDirectoryParameter = "outputdirectory";

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
    /// Convert a relative path to an absolute path, based on the directory
    /// of the instruction file.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="parser">The instruction file parser.</param>
    /// <returns>The absolute path.</returns>
    private static string NormalisePath(this InstructionFileParser parser, string path)
    {
        string? directory = Path.GetDirectoryName(parser.FilePath);
        if (directory == null)
            // Directory will be null if path is root directory (cannot happen),
            // or if the path doesn't contain a directory component, in which
            // case, we can assume that it is in the current directory.
            return path;

        string fullPath = Path.GetFullPath(Path.Combine(directory, path));
        return fullPath;
    }

    /// <summary>
    /// Get the gridlist parameter from an instruction file.
    /// </summary>
    /// <param name="parser">The instruction file parser.</param>
    /// <returns>Absolute path to the gridlist file.</returns>
    public static string GetGridlist(this InstructionFileParser parser)
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(paramGridlist);
        if (parameter != null)
            return parser.NormalisePath(parameter.AsString());
        parameter = parser.GetBlockParameter(parameterBlock, paramGridlistCf, strBlock);
        if (parameter is null)
            throw new InvalidOperationException($"Instruction file {parser.FilePath} does not contain a gridlist parameter");
        return parser.NormalisePath(parameter.AsString());
    }

    /// <summary>
    /// Get the output directory from an instruction file.
    /// </summary>
    /// <param name="parser">The instruction file parser.</param>
    /// <returns>The absolute path to the output directory.</returns>
    /// <exception cref="ArgumentException">Thrown if the output directory is not specified.</exception>
    public static string GetOutputDirectory(this InstructionFileParser parser)
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(outputDirectoryParameter);
        if (parameter is null)
            throw new ArgumentException($"File '{parser.FilePath}' does not specify an output directory");
        return parser.NormalisePath(parameter.AsString());
    }
}
