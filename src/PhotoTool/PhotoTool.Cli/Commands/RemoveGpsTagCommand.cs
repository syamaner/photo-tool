// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMethodReturnValue.Local
namespace PhotoTool.Commands;

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Logging;
using Core;

public class RemoveGpsTagCommand : MetadataCommand 
{
    private readonly IPhotoTagger _photoTagger;
    private readonly ILogger<RemoveGpsTagCommand> _logger;

    public RemoveGpsTagCommand(IPhotoTagger photoTagger, ILogger<RemoveGpsTagCommand> logger) : base("remove", "Removes location metadata from the images.")
    {
        _photoTagger = photoTagger;
        _logger = logger;
        
        // example usage: dotnet PhotoTool.Cli.dll metadata gps remove --image-directory "/where-images-are"
        AddOption( new Option<string>("--image-directory",  "Location for the images to clean up."));
        
        // naming convention matters for binding the options to the method parameters.
        // The default naming convention is to convert the option name to camel case and remove the leading dashes.
        //     example: --image-directory -> imageDirectory
        Handler = CommandHandler.Create(async (string imageDirectory) =>
        {
            await TagImages(imageDirectory);
        });
    }

    private async Task<int> TagImages(string imageDirectory)
    {
        if (!await IsExifToolInstalled())
        {
            _logger.LogError("This application requires Exif Tool to be available in the system. Please install using instructions from https://exiftool.org/install.html.");
            return 1;
        }
        
        _logger.LogInformation(
            "Starting to cleanup location metadata from images in {IMAGES}.", imageDirectory);
        
        try
        {
            var tagImageResult = await _photoTagger.RemoveLocationMetadataFromImages(imageDirectory);
            _logger.LogInformation(
                "Completed removing GPS tags from {TAGGED_COUNT} images. Total image  count: {TOTAL_COUNT}",
                tagImageResult.TaggedImages, tagImageResult.TotalImages);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error removing gps tags from images.");
            return 1;
        }
        
    }
}