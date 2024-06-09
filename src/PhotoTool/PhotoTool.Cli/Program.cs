using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoTool.Core;
using PhotoTool.Core.Gps.IO;
using PhotoTool.Extensions;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var services = new ServiceCollection().AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

services.AddCommands();
 
services.AddScoped<IGpsLogReader, CsvGpsLogReader>();
services.AddScoped<IPhotoTagger, PhotoTagger>();
services.Configure<CliConfigurationOptions>(configuration.GetSection("CliConfiguration"));

var serviceProvider = services.BuildServiceProvider();
var parser = serviceProvider.GetRequiredService<Parser>();

var exitCode =  await parser.InvokeAsync(args);

return exitCode;