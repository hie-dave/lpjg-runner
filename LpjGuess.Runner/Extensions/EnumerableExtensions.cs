namespace LpjGuess.Runner.Extensions;

/// <summary>
/// Extension methods for enumerable types.
/// </summary>
public static class EnumerableExtensions
{
	public static async Task<IEnumerable<TResult>> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
	{
		IEnumerable<Task<TResult>> tasks = source.Select(selector);
		IEnumerable<TResult> results = await Task.WhenAll(tasks);
		return results;
	}

	public static async Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<IEnumerable<TResult>>> selector)
	{
		IEnumerable<Task<IEnumerable<TResult>>> tasks = source.Select(selector);
		IEnumerable<IEnumerable<TResult>> results = await Task.WhenAll(tasks);
		return results.SelectMany(x => x);
	}

	/// <summary>
	/// Create a new dictionary containing the input dictionary, minus the
	/// specified keys.
	/// </summary>
	/// <typeparam name="TKey">The key type of the dictionary.</typeparam>
	/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
	/// <param name="dict">The input dictionary.</param>
	/// <param name="keysToExclude">The keys to exclude from the dictionary.</param>
	/// <returns>A new dictionary containing the input dictionary, minus the specified keys.</returns>
	public static IDictionary<TKey, TValue> ExcludeKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keysToExclude) where TKey : notnull
	{
		Dictionary<TKey, TValue> result = new(dict);
		foreach (TKey key in keysToExclude)
			result.Remove(key);
		return result;
	}

	/// <summary>
	/// Return all combinations of the items in the list.
	/// </summary>
	/// <remarks>
	/// todo: rewrite with less allocations? For now it's enough.
	/// This was taken from here:
	/// https://stackoverflow.com/a/545740
	/// </remarks>
	/// <param name="list">2D input data.</param>
	public static List<List<T>> AllCombinations<T>(this List<List<T>> list)
	{
		// need array bounds checking etc for production
		var combinations = new List<List<T>>();

		// prime the data
		foreach (var value in list[0])
			combinations.Add(new List<T> { value });

		foreach (var set in list.Skip(1))
			combinations = AddExtraSet(combinations, set);

		return combinations;
	}

	private static List<List<T>> AddExtraSet<T>(List<List<T>> combinations, List<T> set)
	{
		var newCombinations = from value in set
							from combination in combinations
							select new List<T>(combination) { value };

		return newCombinations.ToList();
	}
}
