namespace PhotoTool.Extensions;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Commands;

/// <summary>
/// A version of the approach posted at:
///     https://endjin.com/blog/2020/09/simple-pattern-for-using-system-commandline-with-dependency-injection
/// </summary>
public static class CommandLineExtensions
{
    [RequiresUnreferencedCode("TagImagesCommand")]
    public static void AddCommands(this IServiceCollection services)
    {
        var commandType = typeof(Command);

        var commands = typeof(AddGpsTagCommand).Assembly
            .GetExportedTypes()
            .Where(x => commandType.IsAssignableFrom(x) && x is { IsAbstract: false, IsInterface: false });

        foreach (var command in commands)
        {
            services.AddScoped(commandType, command);
        }
        AddCommandParser(services);
    }

    private static void AddCommandParser(this IServiceCollection services)
    {
        // Hierarchical commands to allow CLI usage as following: 
        // photo-tool metadata gps action arguments
        // action: add or remove
        // arguments: relevant directories and other arguments specific to the action
        var metadataCommand = new Command("metadata");
        var gpsCommand = new Command("gps");
        
        metadataCommand.AddCommand(gpsCommand);

        services.AddSingleton<Parser>(provider =>
        {
            var commandLineBuilder = new CommandLineBuilder();

            foreach (var command in provider.GetServices<Command>())
            {
                if (command is MetadataCommand)
                    gpsCommand.AddCommand(command);
                else
                {
                    commandLineBuilder.Command.AddCommand(command);
                }
            }

            commandLineBuilder.Command.AddCommand(metadataCommand);
            return commandLineBuilder.Build();
        });
    }
}