using System.Text;

/// <summary>
/// String helpers. Whitespace is the only word separator.
/// Digits and inner capitals are left untouched.
/// </summary>
public static class StringUtil
{
    /// <summary>
    /// "foo bar baz" -> "FooBarBaz" (first letter capitalized).
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        return BuildCase(input, lowerFirstLetter: false);
    }

    /// <summary>
    /// "Foo bar baz" -> "fooBarBaz" (first letter lowercased).
    /// </summary>
    public static string ToLowerCamelCase(this string input)
    {
        return BuildCase(input, lowerFirstLetter: true);
    }

    private static string BuildCase(string input, bool lowerFirstLetter)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(input.Length);
        bool atWordStart = true;

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                atWordStart = true;
                continue;
            }

            if (atWordStart)
            {
                bool isVeryFirstChar = builder.Length == 0;
                builder.Append(isVeryFirstChar && lowerFirstLetter
                    ? char.ToLowerInvariant(c)
                    : char.ToUpperInvariant(c));
                atWordStart = false;
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}