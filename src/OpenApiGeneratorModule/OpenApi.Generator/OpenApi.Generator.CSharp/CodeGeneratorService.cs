using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Generator
{
    public class CodeGeneratorService : IHostedService, IDisposable
    {
        protected Options Options { get; }
        protected Logger Logger { get; }
        protected Compiler Compiler { get; }

        public CodeGeneratorService(
            Options options,
            Logger<CodeGeneratorService> logger,
            Compiler compiler) =>
            (Options, Logger, Compiler) =
                (options, logger, compiler);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Started execution");
            Logger.LogInformation($"Configuration:\n\t{Options}");
            await Compiler.ExecuteCommands(cancellationToken);
            Logger.LogInformation($"Execution finished");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopped execution");
            await Task.CompletedTask;
        }

        public void Dispose() =>
            GC.SuppressFinalize(this);
    }
}
