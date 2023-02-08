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
	/// Change any relative paths to absolute paths.
	/// </summary>
	public void ConvertToAbsolutePaths()
	{
		string[] parameters = new[]
		{
			"file_met_forcing",
			"file_gridlist",
			"file_soildata",
		};
		string[] paramsToFix = new[]
		{
			"file_soildata",
			"file_ndep"
		};
		foreach (string parameter in parameters)
		{
			string? value = TryGetParameterValue(parameter);
			if (value == null)
				continue;
			string absolute = new DirectoryInfo(value).FullName;
			SetParameterValue(parameter, absolute);
		}
		foreach (string parameter in paramsToFix)
		{
			string? value = TryGetParamValue(parameter);
			if (value == null)
				continue;
			string absolute = new DirectoryInfo(value).FullName;
			SetParamValue(parameter, absolute);
		}
	}

	/// <summary>
	/// Return the value of a param in the instruction file. Throw if it does
	/// not exist.
	/// </summary>
	/// <param name="name">Param name.</param>
	private string GetParamValue(string name)
	{
		string? result = TryGetParamValue(name);
		if (result == null)
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		return result;
	}

	/// <summary>
	/// Return the value of a param value in the instruction file. If it does
	/// not exist, return null.
	/// </summary>
	/// <param name="name">Name of the param.</param>
	private string? TryGetParamValue(string name)
	{
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParamRegex(name);
		Match match = Regex.Match(contents, pattern, opts);
		if (match.Success)
			return match.Groups[2].Value;
		return null;
	}

	/// <summary>
	/// Modify the value of a 'param' option in the .ins file.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <param name="value">New value of the parameter.</param>
	private void SetParamValue(string name, string value)
	{
		string previousValue = GetParamValue(name);
		if (string.Equals(value, previousValue))
			// Parameter already has this value.
			return;

		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParamRegex(name);
		string repl = $@"${{1}}{value}${{3}}";
		string newBuf = Regex.Replace(contents, pattern, repl, opts);
		if (string.Equals(newBuf, contents))
			throw new InvalidOperationException($"Unable to modify '{name}' param (replacement failed)");
		contents = newBuf;
	}

	/// <summary>
	/// Return a regex which may be used to match 'param' values in the
	/// instruction file. Ther return regex contains 3 capture groups:
	/// 1. Everything before the value.
	/// 2. The value.
	/// 3. Everything after the value.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private string GetParamRegex(string name)
	{
		return $@"^([ \t]*param[ \t]+""{name}""[ \t]+\(str[ \t]+"")([^""]+)""\)(.*)";
	}

	/// <summary>
	/// Get a parameter value in the instruction file. Throw if not found.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	private string GetParameterValue(string name)
	{
		string? result = TryGetParameterValue(name);
		if (result == null)
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		return result;
	}

	/// <summary>
	/// Get a parameter value if it exists in the instruction file. If it does
	/// not exist, return null.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	private string? TryGetParameterValue(string name)
	{
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParameterRegex(name);
		Match match = Regex.Match(contents, pattern, opts);
		if (match.Success)
			return match.Groups[2].Value;
		return null;
	}

	/// <summary>
	/// Change a parameter in the instruction file.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="value">Parameter value.</param>
	private void SetParameterValue(string name, string value)
	{
		string previousValue = GetParameterValue(name);
		if (string.Equals(value, previousValue))
			// Nothing to do - parameter already has this value.
			return;
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParameterRegex(name);
		string repl = $@"${{1}}{value}${{3}}";
		// todo: check if a match/replacement occurred.
		string replaced = Regex.Replace(contents, pattern, repl, opts);
		if (string.Equals(replaced, contents))
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		contents = replaced;
	}

	/// <summary>
	/// Return a regex pattern which can be used to search for a string
	/// parameter. The pattern requires the MultiLine option toe be enabled and
	/// contains 3 capture groups:
	/// 1. Everything before the parameter value.
	/// 2. The parameter value.
	/// 3. Everything after the parameter value.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	private string GetParameterRegex(string name)
	{
		return $@"^([ \t]*{name}[ \t]+""?)((?:[ \t]*[^!"" \n])+)(""?[ \t]*[^\n]*)?$";
	}

	/// <summary>
	/// Get a 'parameter does not exist' error message for the specified
	/// parameter.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	private string? ParameterDoesNotExist(string name)
	{
		return $"Parameter '{name}' does not exist";
	}

	/// <summary>
	/// Change a parameter in the instruction file.
	/// </summary>
	/// <param name="factor">The parameter name and new value.</param>
	public void ApplyChange(Factor factor)
	{
		SetParameterValue(factor.Name, factor.Value);
	}

	/// <summary>
	/// Save any pending changes to disk.
	/// </summary>
	public void SaveChanges()
	{
		File.WriteAllText(Path, contents);
	}
}
