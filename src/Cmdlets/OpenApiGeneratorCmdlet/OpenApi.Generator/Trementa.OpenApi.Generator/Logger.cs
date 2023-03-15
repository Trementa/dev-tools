using Microsoft.Extensions.Logging;
using Trementa.OpenApi.Generator;

namespace Trementa.OpenApi.Generator;

public abstract class Logger
{
    readonly ILogger Log;
    readonly Options Opt;
    readonly IList<string> QuietMessages = new List<string>();

    protected Logger(ILogger logger, Options options) => (Log, Opt) = (logger, options);

    bool Quiet => Opt.Quiet;

    public void LogDebug(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogDebug(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogError(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogError(exception, message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogInformation(message, args);
    }

    public void LogTrace(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogTrace(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogWarning(message, args);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        var msLogLevel = (Microsoft.Extensions.Logging.LogLevel)(int)logLevel;
        return Log.IsEnabled(msLogLevel);
    }

    public IDisposable BeginScope(string messageFormat) => Log.BeginScope(messageFormat)!;

    private void Add(string message) => QuietMessages.Add(message);

    public string GetQuietMessages()
        => string.Join(Environment.NewLine, QuietMessages);

    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6,
    }
}

public class Logger<T> : Logger
{
    public Logger(ILogger<T> logger, Options options) : base(logger, options)
    { }
}
