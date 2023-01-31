namespace LpjGuess.Runner.Models.Raw;

/// <summary>
/// All changed values of a single parameter, as specified in the input file.
/// </summary>
internal class Parameter
{
	/// <summary>
	/// Create a new <see cref="Parameter"/> instance.
	/// </summary>
	/// <param name="name">Name of the parameter as it appears in the instruction file.</param>
	/// <param name="values">Values to be applied to the parameter in the runs.</param>
	public Parameter(string name, IReadOnlyCollection<string> values)
	{
		Name = name;
		Values = values;
	}

	/// <summary>
	/// Name of the parameter as it appears in the instruction file.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Values to be applied to the parameter in the runs.
	/// </summary>
	public IReadOnlyCollection<string> Values { get; private init; }
}
