using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using LpjGuess.Runner.Extensions;
using Tomlyn.Syntax;
using Xunit;

namespace LpjGuess.Runner.Tests.Extensions;

public class ReflectionExtensionsTests
{
    #region IsNullable Tests
    
    [Theory]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(string), true)]  // Reference types are nullable
    [InlineData(typeof(object), true)]
    public void IsNullable_Type_ReturnsExpectedResult(Type type, bool expected)
    {
        bool result = type.IsNullable();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(nameof(TestClass.MethodWithNullableParam), true)]
    [InlineData(nameof(TestClass.MethodWithNullableArrayParam), true)]
    [InlineData(nameof(TestClass.MethodWithNotNullableParam), false)]
    public void IsNullable_ParameterInfo_ReturnsExpectedResult(string methodName, bool expected)
    {
        MethodInfo method = typeof(TestClass).GetMethod(methodName)!;
        ParameterInfo param = method.GetParameters()[0];

        bool result = param.IsNullable();

        Assert.Equal(expected, result);
    }

    #endregion

    #region IsType Tests
    
    [Fact]
    public void IsType_ExactMatch_ReturnsTrue()
    {
        Assert.True(typeof(int).IsType<int>());
    }

    [Fact]
    public void IsType_NullableMatch_ReturnsTrue()
    {
        Assert.True(typeof(int?).IsType<int>());
    }

    [Fact]
    public void IsType_NoMatch_ReturnsFalse()
    {
        Assert.False(typeof(string).IsType<int>());
    }

    #endregion

    #region TryConvert Tests
    
    #region Primitive Type Conversion Tests

    [Theory]
    [InlineData((ushort)8192, typeof(int), 8192)]
    [InlineData(0.5, typeof(double), 0.5)]
    [InlineData(42, typeof(string), "42")]
    [InlineData("42", typeof(int), 42)]
    [InlineData("true", typeof(bool), true)]
    public void TryConvert_ValidConversion_Succeeds(object input, Type targetType, object expected)
    {
        bool success = input.TryConvert(targetType, out object? result);
        
        Assert.True(success);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("not a number", typeof(int))]
    [InlineData("not a bool", typeof(bool))]
    public void TryConvert_InvalidConversion_DoesNotThrow(object input, Type targetType)
    {
        bool success = input.TryConvert(targetType, out object? result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_Generic_ValidConversion_Succeeds()
    {
        int input = 42;
        bool success = input.TryConvert(out string? result);
        
        Assert.True(success);
        Assert.Equal("42", result);
    }

    [Fact]
    public void TryConvert_NullInput_ToNullableType_Succeeds()
    {
        object? input = null;
        bool success = input.TryConvert(typeof(int?), out object? result);
        
        Assert.True(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_NullInput_ToValueType_Fails()
    {
        object? input = null;
        bool success = input.TryConvert(typeof(int), out object? result);
        
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryConvert_ToList_Succeeds()
    {
        object input = new object[] { 1, 2, 3 };
        bool success = input.TryConvert(typeof(List<int>), out object? result);
        Assert.True(success);
        Assert.IsType<List<int>>(result);
        Assert.All((List<int>)result, (i, index) => Assert.Equal(index + 1, i));
    }

    #endregion

    #endregion

    #region ToDictionary Tests
    
    [Fact]
    public void ToDictionary_ConvertsDictionaryValues()
    {
        var input = new Dictionary<string, object>
        {
            ["int"] = 42,
            ["short"] = (short)8192,
            ["string"] = "12345"
        };

        var result = (IDictionary)input.ToDictionary(typeof(int));
        
        Assert.IsType<Dictionary<string, int>>(result);
        Assert.Equal(42, ((IDictionary)result)["int"]);
    }

    [Fact]
    public void ToDictionary_InvalidConversion_ThrowsInvalidOperationException()
    {
        var input = new Dictionary<string, object>
        {
            ["invalid"] = "not a number"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => input.ToDictionary(typeof(int)));
        Assert.Contains("invalid", exception.Message);
        Assert.Contains("not a number", exception.Message);
    }

    #endregion

    #region ToList Tests
    
    [Fact]
    public void ToList_ConvertsElements()
    {
        var input = new[] { "1", "2", "3" };
        var result = ReflectionExtensions.ToList(input, typeof(int));
        
        Assert.IsType<List<int>>(result);
        Assert.All(result.Cast<int>(), (i, index) => Assert.Equal(index + 1, i));
    }

    [Fact]
    public void ToList_InvalidConversion_ThrowsFormatException()
    {
        var input = new[] { "not a number" };
        var exception = Assert.Throws<InvalidOperationException>(
            () => ReflectionExtensions.ToList(input, typeof(int)));
        Assert.Contains(input[0], exception.Message);
    }

    #endregion

    #region Dictionary to Object Conversion Tests

    [Fact]
    public void TryConvert_SimpleClass_Succeeds()
    {
        var input = new Dictionary<string, object>
        {
            ["IntValue"] = 42,
            ["StringValue"] = "test"
        };

        bool success = input.TryConvert(typeof(SimpleClass), out object? result);

        Assert.True(success);
        var simpleClass = Assert.IsType<SimpleClass>(result);
        Assert.Equal(42, simpleClass.IntValue);
        Assert.Equal("test", simpleClass.StringValue);
    }

    [Fact]
    public void TryConvert_InitOnlyClass_Succeeds()
    {
        var input = new Dictionary<string, object>
        {
            ["IntValue"] = 42,
            ["StringValue"] = "test"
        };

        bool success = input.TryConvert(typeof(InitOnlyClass), out object? result);

        Assert.True(success);
        var initOnlyClass = Assert.IsType<InitOnlyClass>(result);
        Assert.Equal(42, initOnlyClass.IntValue);
        Assert.Equal("test", initOnlyClass.StringValue);
    }

    [Fact]
    public void TryConvert_ConstructorClass_Succeeds()
    {
        var input = new Dictionary<string, object>
        {
            ["intValue"] = 42,
            ["stringValue"] = "test"
        };

        bool success = input.TryConvert(typeof(ConstructorClass), out object? result);

        Assert.True(success);
        var ctorClass = Assert.IsType<ConstructorClass>(result);
        Assert.Equal(42, ctorClass.IntValue);
        Assert.Equal("test", ctorClass.StringValue);
    }

    [Fact]
    public void TryConvert_DictionaryClass_Succeeds()
    {
        var input = new Dictionary<string, object>
        {
            ["intValue"] = 42,
            ["extraParam1"] = "extra1",
            ["extraParam2"] = 123
        };

        bool success = input.TryConvert(typeof(DictionaryClass), out object? result);

        Assert.True(success);
        var dictClass = Assert.IsType<DictionaryClass>(result);
        Assert.Equal(42, dictClass.IntValue);
        Assert.Equal(2, dictClass.Parameters.Count);
        Assert.Equal("extra1", dictClass.Parameters["extraParam1"]);
        Assert.Equal(123, dictClass.Parameters["extraParam2"]);
    }

    [Fact]
    public void TryConvert_MissingRequiredProperty_ReturnsTrue()
    {
        var input = new Dictionary<string, object>
        {
            ["IntValue"] = 42
            // Missing required StringValue
        };

        // We expect conversion to succeed. The assumption here is that the
        // caller has deliberately setup their class in such a way as to allow
        // for properties to not be set.
        bool success = input.TryConvert(typeof(InitOnlyClass), out object? result);

        Assert.True(success);
        var constructed = Assert.IsType<InitOnlyClass>(result);
        Assert.Equal(42, constructed.IntValue);
    }

    [Fact]
    public void TryConvert_MissingConstructorParameter_ReturnsFalse()
    {
        var input = new Dictionary<string, object>
        {
            ["intValue"] = 42
            // Missing stringValue parameter
        };

        Exception ex = Assert.Throws<InvalidOperationException>(
            () => input.TryConvert(typeof(ConstructorClass), out object? result)
        );
        Assert.Contains(typeof(ConstructorClass).Name, ex.Message);
    }

    [Fact]
    public void TryConvert_InvalidPropertyType_ReturnsFalse()
    {
        // FIXME - should return false.
        var input = new Dictionary<string, object>
        {
            ["IntValue"] = "not an int",
            ["StringValue"] = "test"
        };

        Exception ex = Assert.Throws<InvalidOperationException>(
            () => input.TryConvert(typeof(SimpleClass), out object? result)
        );

        Assert.Contains("IntValue", ex.Message);
    }

    [Fact]
    public void TryConvert_ListOfClasses_Succeeds()
    {
        List<Dictionary<string, object>> input = new()
        {
            new()
            {
                ["IntValue"] = 22,
                ["StringValue"] = "help"
            },
            new()
            {
                ["IntValue"] = 123,
                ["StringValue"] = "asdf"
            }
        };
        bool success = input.TryConvert(out List<SimpleClass>? result);
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(22, result[0].IntValue);
        Assert.Equal("help", result[0].StringValue);
        Assert.Equal(123, result[1].IntValue);
        Assert.Equal("asdf", result[1].StringValue);
    }

    [Fact]
    public void TryConvert_NestedClass_Succeeds()
    {
        Dictionary<string, object> input = new()
        {
            ["IntValue"] = 42,
            ["Nested"] = new Dictionary<string, object>()
            {
                ["X"] = 14,
                ["Y"] = 3
            }
        };
        bool success = input.TryConvert(out NestedClass? result);
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(42, result.IntValue);
        Assert.Equal(14, result.Nested.X);
        Assert.Equal(3, result.Nested.Y);
    }

    #endregion

    #region ToArray Tests
    
    [Fact]
    public void ToArray_GenericList_ReturnsArray()
    {
        var input = new List<int> { 1, 2, 3 };
        var result = ReflectionExtensions.ToArray(input);
        
        Assert.IsType<int[]>(result);
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ToArray_NonGenericList_ReturnsObjectArray()
    {
        var input = new ArrayList { 1, 2, 3 };
        var result = ReflectionExtensions.ToArray(input);
        
        Assert.IsType<object[]>(result);
        Assert.Equal(new object[] { 1, 2, 3 }, result);
    }

    #endregion

    #region IsDictionaryWithKey Tests
    
    [Theory]
    [InlineData(typeof(Dictionary<string, int>), true)]
    [InlineData(typeof(IReadOnlyDictionary<string, bool>), true)]
    [InlineData(typeof(ConcurrentDictionary<string, object>), true)]
    [InlineData(typeof(Dictionary<int, string>), false)]  // Wrong key type
    [InlineData(typeof(List<string>), false)]  // Not a dictionary
    public void IsDictionaryWithKey_ReturnsExpectedResult(Type type, bool expected)
    {
        bool result = type.IsDictionaryWithKey<string>(out Type? valueType);
        
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.NotNull(valueType);
            Assert.Equal(type.GetGenericArguments()[1], valueType);
        }
        else
        {
            Assert.Null(valueType);
        }
    }

    #endregion

    private class TestClass
    {
        public void MethodWithNullableParam(string? param) { }
        public void MethodWithNotNullableParam(string param) { }
        public void MethodWithNullableArrayParam(string[]? param) { }
    }
}
