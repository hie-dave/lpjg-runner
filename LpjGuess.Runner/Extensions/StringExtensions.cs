using System.Text;

namespace LpjGuess.Runner.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Splits a string into substrings, honouring quotes.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <param name="separators">The characters to split on.</param>
    /// <param name="allowSingleQuotes">Whether to allow single quotes as quotes.</param>
    /// <remarks>
    /// Examples:
    /// 
    /// a,b,c -> ["a", "b", "c"]
    /// a,"b, c",d -> ["a", "b, c", "d"]
    /// </remarks>
    /// <returns>The substrings.</returns>
    public static string[] SplitHonouringQuotes(this string str, char[] separators, bool allowSingleQuotes = false)
    {
        List<char> quotes = [ '"' ];
        if (allowSingleQuotes)
            quotes.Add('\'');

        // Parse string.
        bool insideQuotes = false;
        List<string> result = new List<string>();
        int substringStart = 0;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (quotes.Contains(c))
                insideQuotes = !insideQuotes;
            else if (separators.Contains(c) && !insideQuotes)
            {
                result.Add(str.Substring(substringStart, i - substringStart));
                substringStart = i + 1;
            }
        }

        // Add last substring
        result.Add(str.Substring(substringStart));
        return result.ToArray();
    }

    /// <summary>
    /// Convert a PascalCase string to snake_case.
    /// </summary>
    /// <param name="text">The input string in PascalCase.</param>
    /// <returns>The input string, in snake_case.</returns>
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        var sb = new StringBuilder(text.Length + 10);
        sb.Append(char.ToLowerInvariant(text[0]));
        
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(text[i]));
            }
            else
            {
                sb.Append(text[i]);
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Convert the specified text to camelCase.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>The text in camelCase.</returns>
    public static string ToCamelCase(this string text)
    {
        return ToCamelCase(text, false);
    }

    /// <summary>
    /// Convert the specified text to PascalCase.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>The text in PascalCase.</returns>
    public static string ToPascalCase(this string text)
    {
        return ToCamelCase(text, true);
    }

    private static string ToCamelCase(this string text, bool pascal)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder(text.Length + 10);
        char first = pascal ?
            char.ToUpperInvariant(text[0]) :
            char.ToLowerInvariant(text[0]);
        sb.Append(first);

        for (int i = 1; i < text.Length; i++)
        {
            if (text[i] == '_')
            {
                sb.Append(char.ToUpperInvariant(text[i + 1]));
                i++;
            }
            else
            {
                sb.Append(text[i]);
            }
        }

        return sb.ToString();
    }
}
