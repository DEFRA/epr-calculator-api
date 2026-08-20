using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Telemetry.Helpers;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace EPR.Calculator.API.Extensions;

/// <summary>
///     A console logger for local development that includes nicer formatting for telemetry calls and any run-specific
///     information available in the logging event.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DevConsole
{
    private static ITextFormatter DefaultFormatter =>
        new RunInfoFormatter(
            "[{@t:HH:mm:ss} {@l:u3}]{RunInfo}{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)} {@m}\n{@x}",
            DevConsoleTheme.Default);

    private static ITextFormatter TelemetryFormatter =>
        new RunInfoFormatter("[{@t:HH:mm:ss} TEL]{RunInfo}{@m}\n{@x}",
            DevConsoleTheme.Telemetry);

    extension (LoggerSinkConfiguration config)
    {
        public LoggerConfiguration LocalDevConsole()
        {
            var metrics = Matching.FromSource(TelemetryLoggingHost.MetricCategory);

            return config.Logger(lc => lc
                    .Filter.ByExcluding(metrics)
                    .WriteTo.Console(DefaultFormatter))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(metrics)
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(TelemetryFormatter));
        }
    }

    /// <summary>
    ///     This exists because Serilog's TemplateTheme uses a mix of internal/private accessors and can't be read by
    ///     <see cref="RunInfoFormatter" />.
    /// </summary>
    private sealed class DevConsoleTheme (ImmutableDictionary<TemplateThemeStyle, string> ansiStyles)
        : TemplateTheme(ansiStyles)
    {
        // Copy-pasted from Serilog's TemplateThemes.Code since it's internal.
        public static DevConsoleTheme Default => new (new Dictionary<TemplateThemeStyle, string>
        {
            [TemplateThemeStyle.Text]             = "\e[38;5;0253m",
            [TemplateThemeStyle.SecondaryText]    = "\e[38;5;0246m",
            [TemplateThemeStyle.TertiaryText]     = "\e[38;5;0242m",
            [TemplateThemeStyle.Invalid]          = "\e[33;1m",
            [TemplateThemeStyle.Null]             = "\e[38;5;0038m",
            [TemplateThemeStyle.Name]             = "\e[38;5;0081m",
            [TemplateThemeStyle.String]           = "\e[38;5;0216m",
            [TemplateThemeStyle.Number]           = "\e[38;5;0151m",
            [TemplateThemeStyle.Boolean]          = "\e[38;5;0038m",
            [TemplateThemeStyle.Scalar]           = "\e[38;5;0079m",
            [TemplateThemeStyle.LevelVerbose]     = "\e[37m",
            [TemplateThemeStyle.LevelDebug]       = "\e[37m",
            [TemplateThemeStyle.LevelInformation] = "\e[37;1m",
            [TemplateThemeStyle.LevelWarning]     = "\e[38;5;0229m",
            [TemplateThemeStyle.LevelError]       = "\e[38;5;0197m\e[48;5;0238m",
            [TemplateThemeStyle.LevelFatal]       = "\e[38;5;0197m\e[48;5;0238m"
        }.ToImmutableDictionary());

        public static DevConsoleTheme Telemetry => new (new Dictionary<TemplateThemeStyle, string>
        {
            [TemplateThemeStyle.Text]             = "\e[38;5;0111m",
            [TemplateThemeStyle.SecondaryText]    = "\e[38;5;0111m",
            [TemplateThemeStyle.TertiaryText]     = "\e[38;5;0111m",
            [TemplateThemeStyle.Invalid]          = "\e[38;5;0111m",
            [TemplateThemeStyle.Null]             = "\e[38;5;0111m",
            [TemplateThemeStyle.Name]             = "\e[38;5;0111m",
            [TemplateThemeStyle.String]           = "\e[38;5;0111m",
            [TemplateThemeStyle.Number]           = "\e[38;5;0111m",
            [TemplateThemeStyle.Boolean]          = "\e[38;5;0111m",
            [TemplateThemeStyle.Scalar]           = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelVerbose]     = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelDebug]       = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelInformation] = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelWarning]     = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelError]       = "\e[38;5;0111m",
            [TemplateThemeStyle.LevelFatal]       = "\e[38;5;0111m"
        }.ToImmutableDictionary());

        public string? this[TemplateThemeStyle style] => ansiStyles.TryGetValue(style, out var value) ? value : null;
    }

    /// <summary>
    ///     Replaces {RUN_INFO} in the template with the run-specific information, if available.
    /// </summary>
    private sealed class RunInfoFormatter : ITextFormatter
    {
        private const string Token = "{RunInfo}";
        private readonly ExpressionTemplate? prefix;
        private readonly ExpressionTemplate? suffix;
        private readonly DevConsoleTheme theme;

        public RunInfoFormatter(string template, DevConsoleTheme theme)
        {
            this.theme = theme;
            var index = template.IndexOf(Token, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                throw new ArgumentException($@"Template must contain {Token}.", nameof(template));

            if (index > 0)
            {
                prefix = new ExpressionTemplate(
                    template[..index],
                    theme: theme,
                    applyThemeWhenOutputIsRedirected: true);
            }

            var indexEnd = index + Token.Length;

            if (indexEnd < template.Length)
            {
                suffix = new ExpressionTemplate(
                    template[indexEnd..],
                    theme: theme,
                    applyThemeWhenOutputIsRedirected: true);
            }
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            prefix?.Format(logEvent, output);

            if (TryGetRunType(logEvent, out var runType)
                && logEvent.Properties.TryGetValue("run.id", out var runId))
            {
                output.Write(theme[TemplateThemeStyle.TertiaryText]);
                output.Write(" [");
                output.Write(theme[TemplateThemeStyle.String]);
                output.Write(runType);
                output.Write(theme[TemplateThemeStyle.TertiaryText]);
                output.Write(':');
                output.Write(theme[TemplateThemeStyle.Number]);
                runId.Render(output);
                output.Write(theme[TemplateThemeStyle.TertiaryText]);
                output.Write("] ");
            }
            else if (prefix != null && suffix != null)
                output.Write(" ");

            suffix?.Format(logEvent, output);
        }

        // Returns e.g. CR or BR
        private static bool TryGetRunType(LogEvent logEvent, out string runTypeStr)
        {
            runTypeStr = string.Empty;

            if (!logEvent.Properties.TryGetValue("run.type", out var pv))
                return false;

            runTypeStr = pv is ScalarValue { Value: string s } ? s : pv.ToString();

            if (runTypeStr.Length == 0)
                return false;

            runTypeStr = runTypeStr[..1].ToUpperInvariant() + "R";
            return true;
        }
    }
}
