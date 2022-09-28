using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Generator
{
    public class CodeGeneratorService : IHostedService, IDisposable
    {
        protected Options Options { get; }
        protected ILogger Logger { get; }
        protected Compiler Compiler { get; }

        public CodeGeneratorService(
            Options options,
            ILogger<CodeGeneratorService> logger,
            Compiler compiler) =>
            (Options, Logger, Compiler) =
                (options, logger, compiler);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!Options.Quiet)
            {
                Logger.LogInformation("Started execution");
                Logger.LogInformation($"Configuration:\n\t{Options}");
            }

            await Compiler.ExecuteCommands(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopped execution");
            return Task.CompletedTask;
        }

        public void Dispose()
        {}
    }
}
