using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoMapper.Internal;
using Tomlyn.Model;

namespace LpjGuess.Runner.Extensions;

/// <summary>
/// Extension methods for working with TOML objects.
/// </summary>
public static class TomlExtensions
{
    // /// <summary>
    // /// Creates a ParameterSet instance from a TOML table.
    // /// </summary>
    // /// <typeparam name="T">The target type (should be ParameterSet)</typeparam>
    // /// <param name="table">The TOML table containing parameter set data</param>
    // /// <returns>A ParameterSet instance</returns>
    // private static T CreateParameterSet<T>(TomlTable table) where T : class
    // {
    //     // Extract the name
    //     string name = "";
    //     if (table.TryGetValue("name", out var nameValue) && nameValue is string nameStr)
    //     {
    //         name = nameStr;
    //     }
        
    //     // Create parameters dictionary
    //     var parameters = new Dictionary<string, object[]>();
    //     foreach (var kvp in table)
    //     {
    //         if (kvp.Key != "name" && kvp.Value is TomlArray array)
    //         {
    //             parameters[kvp.Key] = array.Cast<object>().ToArray();
    //         }
    //     }
        
    //     // Find the constructor that takes name and parameters
    //     var constructor = typeof(T).GetConstructors()
    //         .FirstOrDefault(c => {
    //             var p = c.GetParameters();
    //             return p.Length == 2 && 
    //                    p[0].ParameterType == typeof(string) && 
    //                    p[1].ParameterType.Name.Contains("Dictionary");
    //         });
            
    //     if (constructor == null)
    //     {
    //         throw new InvalidOperationException($"Could not find suitable constructor for {typeof(T).Name}");
    //     }
        
    //     return (T)constructor.Invoke([name, parameters]);
    // }

    /// <summary>
    /// Converts a TOML table array to a collection of type T.
    /// </summary>
    /// <typeparam name="T">The target element type</typeparam>
    /// <param name="tableArray">The TOML table array to convert</param>
    /// <returns>A collection of T instances</returns>
    public static IReadOnlyCollection<T> ToImmutableCollection<T>(this TomlTableArray tableArray) where T : class
    {
        var result = new List<T>();
        foreach (var table in tableArray)
        {
            result.Add(table.ToComplexType<T>());
        }
        return result;
    }
}
