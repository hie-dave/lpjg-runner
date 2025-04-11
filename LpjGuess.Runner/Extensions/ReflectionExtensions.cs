using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LpjGuess.Runner.Extensions;

public static class ReflectionExtensions
{
    private static readonly HashSet<Type> dictionaryTypes = [
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>),
        typeof(Dictionary<,>),
        typeof(ReadOnlyDictionary<,>),
        typeof(ImmutableDictionary<,>),
        typeof(ConcurrentDictionary<,>),
        typeof(SortedDictionary<,>),
        typeof(ImmutableSortedDictionary<,>),
    ];

    private static readonly HashSet<Type> nonGenericCollectionTypes = [
        typeof(IEnumerable),
        typeof(ICollection),
        typeof(IList),
    ];

    private static readonly HashSet<Type> genericCollectionTypes = [
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IList<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>),
        typeof(List<>),
        typeof(Collection<>),
        typeof(ReadOnlyCollection<>),
        typeof(ImmutableList<>),
        typeof(ConcurrentBag<>),
        typeof(ConcurrentQueue<>),
        typeof(ConcurrentStack<>),
    ];

    private static readonly HashSet<Type> numericTypes = [
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
    ];

    private static readonly NullabilityInfoContext nullabilityContext = new();

    public static bool IsNullable(this Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsNullable(this ParameterInfo param)
    {
        NullabilityInfo nullability = nullabilityContext.Create(param);
        return nullability.ReadState == NullabilityState.Nullable;
    }

    /// <summary>
    /// Check if the specified type is the target type (nullable or not).
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True iff the type is the target type or its nullable version.</returns>
    public static bool IsType<T>(this Type type)
    {
        return type == typeof(T) || Nullable.GetUnderlyingType(type) == typeof(T);
    }

    /// <summary>
    /// Try to convert the object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="value">The object to be converted.</param>
    /// <param name="result">The result value, or null if the object can't be converted to the destination type.</param>
    /// <returns>True iff the object was successfully converted.</returns>
    public static bool TryConvert<T>(this object value, [NotNullWhen(true)] out T? result)
    {
        if (TryConvert(value, typeof(T), out object? res))
        {
            result = (T)res;
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Try to convert an object to another type.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="targetType">The type to which the value will be converted.</param>
    /// <param name="result">The result value, or null if the object can't be converted to the destination type.</param>
    /// <returns>True iff the object was successfully converted.</returns>
    public static bool TryConvert(this object? value, Type targetType, [NotNullWhen(true)] out object? result)
    {
        result = null;
        
        if (value == null)
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

        if (targetType == value.GetType())
        {
            result = value;
            return true;
        }

        // Handle simple type conversions
        if (targetType == typeof(string))
        {
            result = value.ToString() ?? string.Empty;
            return true;
        }

        if (value is IConvertible)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(value.GetType()) && converter.IsValid(value))
            {
                result = converter.ConvertFrom(value)!;
                return true;
            }
        }

        // Handle array conversions.
        if (targetType.IsArray)
        {
            Type elementType = targetType.GetElementType()!;
            if (value is IEnumerable collection)
            {
                result = collection.ToList(elementType).ToArray();
                return true;
            }
        }
        
        // Handle conversion to generic collections.
        if (targetType.IsGenericType)
        {
            Type[] typeParameters = targetType.GetGenericArguments();
            if (typeParameters.Length == 1)
            {
                bool isGenericCollection = genericCollectionTypes
                    .Select(t => t.MakeGenericType([typeParameters[0]]))
                    .Any(t => t.IsAssignableTo(targetType));

                if (isGenericCollection && value is IEnumerable enumerable)
                {
                    Type elementType = typeParameters[0];
                    result = ToList(enumerable, elementType);
                    return true;
                }
            }
        }
        
        // Handle complex type conversions
        if (value is IDictionary<string, object> nestedTable && !targetType.IsPrimitive && targetType != typeof(string))
        {
            result = ToComplexType(nestedTable, targetType);
            return true;
        }

        try
        {
            result = Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Convert a dictionary with one value type to a dictionary with another
    /// value type, converting any values to the output value type as required.
    /// </summary>
    /// <typeparam name="TKey">Key type of the input dictionary.</typeparam>
    /// <typeparam name="TValue">Value type of the input dictionary.</typeparam>
    /// <param name="table">The dictionary to convert.</param>
    /// <param name="valueType">The type to which the values should be converted.</param>
    /// <returns>A generic dictionary with key type TKey and value type valueType.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an element cannot be converted to the specified type.</exception>
    public static object ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> table, Type valueType)
    {
        Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TKey), valueType);
        object dict = Activator.CreateInstance(dictionaryType)!;
        MethodInfo addMethod = dictionaryType.GetMethod("Add")!;

        // Need to convert values to valueType.
        foreach (TKey key in table.Keys)
        {
            // TODO: logging.
            if (!TryConvert(table[key], valueType, out object? convertedValue))
                throw new InvalidOperationException($"Cannot convert value {table[key]} to type {valueType.Name} for dictionary parameter {key}.");
            addMethod.Invoke(dict, [key, convertedValue!]);
        }

        return dict;
    }

    /// <summary>
    /// Convert any collection to a generic list with the specified element
    /// type, converting any elements to the correct type as required.
    /// </summary>
    /// <param name="collection">The collection to be converted.</param>
    /// <param name="elementType">The element type of the destination list.</param>
    /// <returns>A generic list containing the converted elements.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an element cannot be converted to the specified type.</exception>
    public static IList ToList(this IEnumerable collection, Type elementType)
    {
        Type type = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList)Activator.CreateInstance(type)!;
        foreach (object item in collection)
        {
            // TODO: logging.
            if (!TryConvert(item, elementType, out object? convertedValue))
                throw new InvalidOperationException($"Cannot convert value {item} to type {elementType.Name} for list.");
            list.Add(convertedValue);
        }
        return list;
    }

    /// <summary>
    /// Converts an <see cref="IList"/> to an <see cref="Array"/>.
    /// </summary>
    /// <param name="list">The list to convert</param>
    /// <returns>An array containing the elements of the list</returns>
    public static Array ToArray(this IList list)
    {
        // Try to use the ToArray method if available
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        IEnumerable<MethodInfo> methods = list
            .GetType()
            .GetMethods(flags)
            .Where(m => m.Name == "ToArray" && m.GetParameters().Length == 0);
        MethodInfo method = methods.First();
        if (method != null)
        {
            return (Array)method.Invoke(list, null)!;
        }

        // Fallback implementation for generic lists.
        if (list.GetType().IsGenericType)
        {
            Type elementType = list.GetType().GetGenericArguments()[0];
            Array array = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }
            return array;
        }

        // We have a non-generic IList, so we have no way of knowing the correct
        // element type.
        throw new InvalidOperationException($"Cannot convert {list.GetType().Name} to Array.");
    }

    /// <summary>
    /// Converts a TOML table to an instance of type T, supporting immutable types with constructor parameters.
    /// </summary>
    /// <typeparam name="T">The target type to convert to</typeparam>
    /// <param name="table">The TOML table to convert</param>
    /// <returns>An instance of T populated with values from the TOML table</returns>
    public static T ToComplexType<T>(this IDictionary<string, object> table) where T : class
    {
        return (T)ToComplexType(table, typeof(T));
    }

    /// <summary>
    /// Converts a dictionary to an instance of a given type, mapping keys to
    /// either settable/init-able properties or parameters of a constructor, if
    // one is available.
    /// </summary>
    /// <param name="table">The dictionary to convert</param>
    /// <returns>
    /// An instance of the specified type populated with values from the table.
    /// </returns>
    public static object ToComplexType(this IDictionary<string, object> table, Type targetType)
    {
        // First, try to find a suitable constructor.
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        IEnumerable<ConstructorInfo> constructors = targetType
            .GetConstructors(flags)
            .OrderByDescending(c => c.GetParameters().Length);

        foreach (ConstructorInfo constructor in constructors)
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                // If parameterless constructor exists, create instance and set
                // properties. Remember, we ordered the constructors by number
                // of parameters, so if we get to here, there are no more
                // constructors to try.
                object instance = constructor.Invoke(Array.Empty<object>());
                SetProperties(instance, table);
                return instance;
            }

            // Try to match constructor parameters with TOML table entries
            object?[] args = new object?[parameters.Length];
            bool canUseConstructor = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo param = parameters[i];
                string paramName = param.Name!;
                IEnumerable<string> namesToTry = GetDictNames(paramName, true);
                string? tomlKey = namesToTry.FirstOrDefault(table.ContainsKey);

                // Check if we have a matching TOML key
                if (tomlKey != null && table.TryGetValue(tomlKey, out object? value))
                {
                    // Parameter exists in the TOML.
                    // Try to convert the TOML value to the parameter type.
                    if (value.TryConvert(param.ParameterType, out object? convertedValue))
                    {
                        args[i] = convertedValue;
                    }
                    else
                    {
                        // TODO: logging.
                        Console.WriteLine($"Cannot use constructor {constructor} for type {targetType.Name}: cannot convert value {value} to type {param.ParameterType.Name} for parameter {paramName}.");
                        canUseConstructor = false;
                        break;
                    }
                }
                else if (param.ParameterType.IsDictionaryWithKey<string>(out Type? valueType))
                {
                    // Parameter is an IDictionary, and does not exist in TOML.
                    // Therefore, we populate it with all remaining TOML
                    // parameters.
                    IEnumerable<string> explicitParameters = parameters
                        .Select(p => GetDictNames(p.Name!, true)
                                    .FirstOrDefault(table.ContainsKey))
                        .Where(n => n != null)
                        .Select(n => n!)
                        .ToArray();
                    args[i] = table.ExcludeKeys(explicitParameters)
                                   .ToDictionary(valueType);
                }
                else if (param.HasDefaultValue)
                {
                    // Parameter has a default value and does not exist in TOML.
                    // Therefore use the default value.
                    args[i] = param.DefaultValue!;
                }
                else if (param.IsNullable())
                {
                    // Parameter is optional, and does not exist in TOML.
                    // Therefore set to null.
                    args[i] = null;
                }
                else
                {
                    // Parameter does not exist in the TOML, does not have a
                    // default value, and is not optional (ie nullable).
                    // Therefore we cannot use this constructor.
                    // TODO: implement logging and write a suitable message here.
                    Console.WriteLine($"Cannot use constructor {constructor} for type {targetType.Name}: parameter {param.Name} is required but does not exist in the input table.");
                    canUseConstructor = false;
                    break;
                }
            }

            if (canUseConstructor)
            {
                return constructor.Invoke(args);
            }
        }

        throw new InvalidOperationException($"Could not find a suitable constructor for type {targetType.Name}");
    }

    /// <summary>
    /// Get the set of names which may appear in a dictionary which would match
    /// the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter name.</param>
    /// <returns>All valid names of this parameter in a dictionary.</returns>
    private static HashSet<string> GetDictNames(string parameter, bool allowCamel)
    {
        HashSet<string> names = [
            parameter,
            parameter.ToPascalCase(),
            parameter.ToSnakeCase()
        ];
        if (allowCamel)
            names.Add(parameter.ToCamelCase());
        return names;
    }

    /// <summary>
    /// Set the properties on a constructed object from the given dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the constructed object.</typeparam>
    /// <param name="instance">The constructed object.</param>
    /// <param name="table">A dictionary mapping property names to values.</param>
    /// <exception cref="InvalidOperationException">Thrown if a property value cannot be converted to the property type.</exception>
    private static void SetProperties<T>(T instance, IDictionary<string, object> table) where T : class
    {
        // Get all writable properties. This includes init-only properties.
        IEnumerable<PropertyInfo> properties = instance
            .GetType()
            .GetProperties()
            .Where(p => p.CanWrite);

        foreach (PropertyInfo property in properties)
        {
            string propertyName = property.Name;
            // Allow snake_case for input parameters from toml.
            IEnumerable<string> namesToTry = GetDictNames(propertyName, false);
            string? tomlKey = namesToTry.FirstOrDefault(table.ContainsKey);

            if (tomlKey != null && table.TryGetValue(tomlKey, out object? value))
            {
                if (value.TryConvert(property.PropertyType, out object? convertedValue))
                    property.SetValue(instance, convertedValue);
                else
                    throw new InvalidOperationException($"Cannot convert value {value} to type {property.PropertyType.Name} for property {propertyName} of type {typeof(T).Name}.");
            }
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(property.PropertyType))
            {
                // Populate the dictionary with all remaining TOML parameters
                IEnumerable<string> explicitProperties = properties.Select(p => p.Name.ToSnakeCase()).ToArray();
                IDictionary<string, object> dict = table.ExcludeKeys(explicitProperties);
                property.SetValue(instance, dict);
            }
        }
    }

    /// <summary>
    /// Checks if a type is a dictionary-like type with the specified key type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <param name="valueType">The value type of the dictionary, if it is a dictionary</param>
    /// <returns>True if the type is a dictionary-like type with the specified key type</returns>
    public static bool IsDictionaryWithKey<TKey>(this Type type, [NotNullWhen(true)] out Type? valueType)
    {
        valueType = null;

        if (!type.IsGenericType)
            return false;

        Type[] genericArgs = type.GetGenericArguments();
        
        // Check if it has at least two generic arguments and the first one is the specified key type
        if (genericArgs.Length < 2 || genericArgs[0] != typeof(TKey))
            return false;

        valueType = genericArgs[1];

        Type value = valueType;
        IEnumerable<Type> genericTypes = dictionaryTypes.Select(t => t.MakeGenericType(typeof(TKey), value));

        return genericTypes.Any(g => g.IsAssignableFrom(type));
    }
}
