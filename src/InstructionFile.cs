using System.Text.RegularExpressions;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner;

/// <summary>
/// Encapsulates an instruction file.
/// </summary>
/// <remarks>
/// This is really quick and dirty. This really should be improved because
/// performance is not going to be great here.
/// </remarks>
public class InstructionFile
{
	/// <summary>
	/// Path to the file.
	/// </summary>
	/// <value></value>
	public string Path { get; private init; }

	/// <summary>
	/// File contents.
	/// </summary>
	private string contents;

	/// <summary>
	/// Create a new <see cref="InstructionFile"/> instance.
	/// </summary>
	/// <param name="path">Path to the instruction file.</param>
	public InstructionFile(string path)
	{
		Path = path;
		contents = File.ReadAllText(path);
	}

	/// <summary>
	/// Copy the text of all imported .ins files, recursively, into this file.
	/// </summary>
	public void Flatten()
	{
		string pattern = $@"^[ \t]*import[ \t]+""([^""]+)"".*\n";
		RegexOptions opts = RegexOptions.Multiline;
		Match match;
		while ( (match = Regex.Match(contents, pattern, opts)) != Match.Empty)
		{
			string file = match.Groups[1].Value;
			contents = contents.Remove(match.Index, match.Length);
			contents = contents.Insert(match.Index, File.ReadAllText(file));
		}
	}

	/// <summary>
	/// Change a parameter in the instruction file.
	/// </summary>
	/// <param name="factor">The parameter name and new value.</param>
	public void ApplyChange(Factor factor)
	{
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = $@"^([ \t]*{factor.Name}[ \t]+)[^!]+(!.*)?";
		string repl = $@"\1{factor.Value}\2";
		// todo: check if a match/replacement occurred.
		string replaced = Regex.Replace(contents, pattern, repl, opts);
		if (!string.Equals(replaced, contents))
			throw new InvalidOperationException($"Parameter '{factor.Name}' does not exist");
		contents = replaced;
	}

	/// <summary>
	/// Save any pending changes to disk.
	/// </summary>
	public void SaveChanges()
	{
		File.WriteAllText(Path, contents);
	}
}
