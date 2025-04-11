using LpjGuess.Runner.Extensions;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace LpjGuess.Runner.Tests;

public class StringExtensionTests
{
    [Theory]
    [InlineData("xyz", "xyz")]
    [InlineData("a,b,c", "a", "b", "c")]
    [InlineData("a,\"b,c\",d", "a", "\"b,c\"", "d")]
    [InlineData(",ab,c", "", "ab", "c")]
    [InlineData("x,y,z,", "x", "y", "z", "")]
    public void TestSplitHonouringQuotes(string input, params string[] expectedOutput)
    {
        string[] result = input.SplitHonouringQuotes([',']);
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData('!', "a!b!c", "a", "b", "c")]
    [InlineData('!', "a!\"b!,c\"!d", "a", "\"b!,c\"", "d")]
    public void SplitHonouringQuotes_HandlesOtherDelimiters(char delim, string input, params string[] expectedOutput)
    {
        string[] result = input.SplitHonouringQuotes([delim]);
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("x,'y,z'", "x", "'y,z'")]
    [InlineData("a,'b,c',\"d,e\",f", "a", "'b,c'", "\"d,e\"", "f")]
    public void SplitHonouringQuotes_HandlesSingleQuotes(string input, params string[] expectedOutput)
    {
        string[] result = input.SplitHonouringQuotes([','], true);
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("this_is_a_name", "thisIsAName")]
    [InlineData("ThisIsPascalCase", "thisIsPascalCase")]
    public void TestToCamelCase(string input, string expected)
    {
        string actual = input.ToCamelCase();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("this_is_a_name", "ThisIsAName")]
    [InlineData("ThisIsPascalCase", "ThisIsPascalCase")]
    public void TestToPascalCase(string input, string expected)
    {
        string actual = input.ToPascalCase();
        Assert.Equal(expected, actual);
    }
}
