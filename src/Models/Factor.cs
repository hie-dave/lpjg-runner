namespace LpjGuess.Runner.Models;

/// <summary>
/// A name/value pair 
/// </summary>
public class Factor
{
	/// <summary>
	/// Name of the parameter as it appears in the instruction file.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Value of the parameter.
	/// </summary>
	public string Value { get; private init; }

	/// <summary>
	/// Create a new <see cref="Factor"/> instance.
	/// </summary>
	/// <param name="name">Name of the parameter as it appears in the instruction file.</param>
	/// <param name="value">Value of the parameter.</param>
	public Factor(string name, string value)
	{
		Name = name;
		Value = value;
	}
}
