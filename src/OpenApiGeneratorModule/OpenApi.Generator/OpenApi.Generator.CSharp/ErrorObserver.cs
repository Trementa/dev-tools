using System;
using System.Collections.Generic;

namespace OpenApi.Generator
{
    public class ErrorObserver : IObserver<Error>
    {
        protected readonly Logger Logger;

        public ErrorObserver(Logger logger) =>
            Logger = logger;

        public void OnCompleted()
        {
            if (Errors.Count == 0)
                Logger.LogInformation("Code generation completed without errors.");
            else if (Errors.Count > 0)
            {
                var log = Logger.BeginScope("Errors");
                Logger.LogError(Errors, e => e.Message);
                Logger.LogError($"Code generation completed with { Errors.Count } errors");
            }
        }

        public void OnError(Exception error) =>
            Logger.LogError("Error when handling Error...");

        public void OnNext(Error value) =>
            Errors.Add(value);

        public List<Error> Errors { get; private set; } = new List<Error>();
    }

}
