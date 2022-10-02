using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OpenApi.Generator;

public abstract class Logger
{
    readonly ILogger Log;
    readonly Options Opt;
    readonly List<string> QuietMessages = new List<string>();

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

    public void LogWarning(string message, params object[] args)
    {
        if (Quiet)
            Add(message);
        else
            Log.LogWarning(message, args);
    }

    public IDisposable BeginScope(string messageFormat) => Log.BeginScope(messageFormat);

    private void Add(string message) => QuietMessages.Add(message);
}

public class Logger<T> : Logger
{
    public Logger(ILogger<T> logger, Options options) : base(logger, options)
    { }
}
