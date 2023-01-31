using System.Text.RegularExpressions;

namespace LpjGuess.Runner.Models;

/// <summary>
/// This class encapsulates a list of changed .ins file parameters to be applied
/// to an LPJ-Guess simulation.
/// </summary>
public class Factorial
{
	/// <summary>
	/// Parameter changes to be applied to an LPJ-Guess simulation.
	/// </summary>
	public IReadOnlyCollection<Factor> Factors { get; private init; }

	/// <summary>
	/// Create a new <see cref="Factorial"/> instance.
	/// </summary>
	/// <param name="factors">Parameter changes to be applied to an LPJ-Guess simulation.</param>
	public Factorial(IReadOnlyCollection<Factor> factors)
	{
		Factors = factors;
	}
}
