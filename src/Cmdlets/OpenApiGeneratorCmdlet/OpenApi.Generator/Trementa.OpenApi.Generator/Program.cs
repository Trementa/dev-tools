﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Trementa.OpenApi.Generator;

public static class Program
{
    public static void Main(params string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        if (args.Length == 0 || string.Equals(args[0], "help", StringComparison.InvariantCultureIgnoreCase))
            ShowHelp();
        else
            Run(args);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.IsTerminating)
            Console.Error.WriteLine("Process terminating");

        Console.Error.WriteLine($"Error: {e}");
    }

    static void ShowHelp() =>
        Console.Write(Options.Help);

    static void Run(string[] args) =>
        CreateHostBuilder(args).Build().Start();

    static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                    config.AddCommandLineMapping(args))
                .UseDefaultServiceProvider((context, options) =>
                    options.ValidateOnBuild = false)
            .ConfigureServices((hostContext, services) =>
                ConfigureDefaultServices(hostContext.Configuration, services));

    static void ConfigureDefaultServices(IConfiguration configuration, IServiceCollection services)
    {
        services
            .AddLogging()
            .AddTransient(typeof(Logger<>))
            .AddGeneratorServices(configuration);
    }
}
