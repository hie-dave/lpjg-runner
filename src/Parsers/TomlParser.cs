using LpjGuess.Runner.Extensions;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Models.Raw;
using Tomlyn;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// This class can parse a .toml input file.
/// </summary>
internal class TomlParser : IParser
{
	/// <inheritdoc />
	public Instructions Parse(string file)
	{
		try
		{
			InputModel model = Toml.ToModel<InputModel>(File.ReadAllText(file));
			return Parse(model);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to parse input file '{file}'", error);
		}
	}

	/// <summary>
	/// Parse an instructions object from the raw input object. This requires
	/// some manual validation of inputs not covered by the library.
	/// </summary>
	/// <param name="model">The raw user input object.</param>
	private Instructions Parse(InputModel model)
	{
		if (model.Insfiles.Count == 0)
			throw new InvalidOperationException($"No instruction files provided");

		IReadOnlyCollection<Factorial> combinations = GetParameters(model.Parameters);
		RunSettings settings = new RunSettings(model);
		return new Instructions(model.Insfiles, model.Pfts, combinations, settings);
	}

	/// <summary>
	/// Get all combinations of factors from the user inputs.
	/// </summary>
	/// <param name="parameters">The parameters as they appear in the user input object.</param>
	private IReadOnlyCollection<Factorial> GetParameters(IDictionary<string, List<string>> parameters)
	{
		if (parameters.Count == 0)
			return new List<Factorial>();

		// Convert dictionary to 2D list of factors.
		List<List<Factor>> factors = new List<List<Factor>>();
		foreach ((string key, IReadOnlyList<string> values) in parameters)
			factors.Add(new List<Factor>(values.Select(v => new Factor(key, v))));

		// Return all combinations thereof.
		List<List<Factor>> combinations = factors.AllCombinations();
		return combinations.Select(c => new Factorial(c)).ToList();
	}
}
