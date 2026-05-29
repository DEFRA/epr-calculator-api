using System.Diagnostics.CodeAnalysis;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace EPR.Calculator.Service.Function.Logging;

[ExcludeFromCodeCoverage]
public static class DevConsole
{
    // Copied from Serilog's TemplateTheme.Code since it's not public.
    private static readonly Dictionary<TemplateThemeStyle, string> Styles = new()
    {
        [TemplateThemeStyle.Text] = "\u001B[38;5;0253m",
        [TemplateThemeStyle.SecondaryText] = "\u001B[38;5;0246m",
        [TemplateThemeStyle.TertiaryText] = "\u001B[38;5;0242m",
        [TemplateThemeStyle.Invalid] = "\u001B[33;1m",
        [TemplateThemeStyle.Null] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Name] = "\u001B[38;5;0081m",
        [TemplateThemeStyle.String] = "\u001B[38;5;0216m",
        [TemplateThemeStyle.Number] = "\u001B[38;5;151m",
        [TemplateThemeStyle.Boolean] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Scalar] = "\u001B[38;5;0079m",
        [TemplateThemeStyle.LevelVerbose] = "\u001B[37m",
        [TemplateThemeStyle.LevelDebug] = "\u001B[37m",
        [TemplateThemeStyle.LevelInformation] = "\u001B[37;1m",
        [TemplateThemeStyle.LevelWarning] = "\u001B[38;5;0229m",
        [TemplateThemeStyle.LevelError] = "\u001B[38;5;0197m\u001B[48;5;0238m",
        [TemplateThemeStyle.LevelFatal] = "\u001B[38;5;0197m\u001B[48;5;0238m"
    };

    private static readonly TemplateTheme Theme = new(Styles);

    public static ITextFormatter Logger()
    {
        const string prefix = "[{@t:HH:mm:ss} {@l:u3}]";
        const string suffix = " {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)} {@m}\n{@x}";

        return new ConsoleFormatter(
            new ExpressionTemplate(prefix, theme: Theme),
            new ExpressionTemplate(suffix, theme: Theme),
            Styles);
    }

    public static ITextFormatter Telemetry()
    {
        const string prefix = "\x1b[38;5;0111m[{@t:HH:mm:ss} TEL]";
        const string suffix = "\x1b[38;5;0111m {@m}\n{@x}";

        return new ConsoleFormatter(
            new ExpressionTemplate(prefix),
            new ExpressionTemplate(suffix),
            Styles);
    }

    private sealed class ConsoleFormatter(
        ExpressionTemplate prefix,
        ExpressionTemplate suffix,
        IReadOnlyDictionary<TemplateThemeStyle, string> styles)
        : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            prefix.Format(logEvent, output);

            if (TryGetRunType(logEvent, out var runType)
                && logEvent.Properties.TryGetValue("RunId", out var runId))
            {
                output.Write(styles.GetValueOrDefault(TemplateThemeStyle.TertiaryText));
                output.Write(" [");
                output.Write(styles.GetValueOrDefault(TemplateThemeStyle.String));
                output.Write(runType);
                output.Write(styles.GetValueOrDefault(TemplateThemeStyle.TertiaryText));
                output.Write(':');
                output.Write(styles.GetValueOrDefault(TemplateThemeStyle.Number));
                runId.Render(output);
                output.Write(styles.GetValueOrDefault(TemplateThemeStyle.TertiaryText));
                output.Write(']');
            }

            suffix.Format(logEvent, output);
        }

        private static bool TryGetRunType(LogEvent logEvent, out string runTypeStr)
        {
            runTypeStr = string.Empty;

            if (!logEvent.Properties.TryGetValue("RunType", out var pv))
                return false;

            runTypeStr = pv is ScalarValue { Value: string s } ? s : pv.ToString();
            runTypeStr = runTypeStr.PadRight(1)[..1].ToUpperInvariant() + "R";
            return true;
        }
    }
}
