using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenApi.Generator
{
    public class ErrorObserver : IObserver<Error>
    {
        protected readonly ILogger Logger;
        protected readonly Options Options;

        public ErrorObserver(Options options, ILogger logger) =>
            (Options, Logger) =
            (options, logger);

        public void OnCompleted()
        {
            if (Errors.Count == 0 && !Options.Quiet)
                Logger.LogInformation("Code generation completed without errors.");
            else if (Errors.Count > 0)
            {
                // Always output errors
                foreach (var error in Errors)
                {
                    Logger.LogError(error.Message);
                }
                Logger.LogError($"Code generation completed with { Errors.Count } errors");
            }
        }

        public void OnError(Exception error)
        {
            if (!Options.Quiet)
                Logger.LogError("Error when handling Error...");
        }

        public void OnNext(Error value)
        {
            Errors.Add(value);
        }

        public List<Error> Errors { get; private set; } = new List<Error>();
    }

}
