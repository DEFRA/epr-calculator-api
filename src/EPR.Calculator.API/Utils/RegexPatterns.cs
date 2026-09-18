using System.Text.RegularExpressions;

namespace EPR.Calculator.API.Utils;

public static partial class RegexPatterns
{
    [GeneratedRegex(@"[^0-9\,\.\-]")]
    public static partial Regex NonDecimalChars();
}
