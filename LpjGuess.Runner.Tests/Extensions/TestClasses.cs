using System.Collections.Generic;

namespace LpjGuess.Runner.Tests.Extensions;

// Test classes for dictionary conversion
public record class SimpleClass
{
    public int IntValue { get; set; }
    public string StringValue { get; set; } = "";
}

public record class InitOnlyClass
{
    public required int IntValue { get; init; }
    public required string StringValue { get; init; }
}

public record class ConstructorClass
{
    public int IntValue { get; }
    public string StringValue { get; }

    public ConstructorClass(int intValue, string stringValue)
    {
        IntValue = intValue;
        StringValue = stringValue;
    }
}

public record class DictionaryClass
{
    public int IntValue { get; }
    public IDictionary<string, object> Parameters { get; }

    public DictionaryClass(int intValue, IDictionary<string, object> parameters)
    {
        IntValue = intValue;
        Parameters = parameters;
    }
}

public record class NestedClass
{
    public int IntValue { get; }
    public SubClass Nested { get; }
    public NestedClass(int intValue, SubClass nested)
    {
        IntValue = intValue;
        Nested = nested;
    }
}

public record class SubClass
{
    public int X { get; init; }
    public int Y { get; init; }
}
