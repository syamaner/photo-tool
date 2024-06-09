// ReSharper disable UnusedMethodReturnValue.Local
namespace PhotoTool.Commands;

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Logging;
using Core;

public class AddGpsTagCommand: MetadataCommand
{
    private readonly IPhotoTagger _photoTagger;
    private readonly ILogger<AddGpsTagCommand> _logger;

    public AddGpsTagCommand(IPhotoTagger photoTagger, ILogger<AddGpsTagCommand> logger) : base("add", "Add GPS tags to images where a matching log entry is found. If an image already has GPS tags, they will be preserved.")
    {
        _photoTagger = photoTagger;
        _logger = logger;
        // Options below enable calling out cli as following:
        // dotnet PhotoTool.Cli.dll metadata gps add --image-directory "/where-images-are" --gps-log-directory "/where-csv-logs-are" --max-matching-seconds 5

        AddOption( new Option<string>("--image-directory",  "Location for the images to tag."));
        AddOption( new Option<string>("--gps-log-directory",  "Location where GPS logs are located."));
        AddOption( new Option<int>("--max-matching-seconds",() => 10, "Maximum time difference allowed between the photo timestamp and nearest GPS log entry timestamp in UTC."));

        // naming convention matters for binding the options to the method parameters.
        // The default naming convention is to convert the option name to camel case and remove the leading dashes.
        //     example: --image-directory -> imageDirectory, --gps-log-directory -> gpsLogDirectory, --max-matching-seconds -> maxMatchingSeconds
        Handler = CommandHandler.Create(async (string imageDirectory, string gpsLogDirectory, int maxMatchingSeconds) =>
        {
            await TagImages(imageDirectory, gpsLogDirectory, maxMatchingSeconds);
        });
    }

    private async Task<int> TagImages(string imageDirectory, string gpsLogDirectory, int maxMatchingSeconds)
    {
        if (!await IsExifToolInstalled())
        {
            _logger.LogError("This application requires Exif Tool to be available in the system. Please install using instructions from https://exiftool.org/install.html.");
            return 1;
        }

        _logger.LogInformation(
            "Starting to process image directory {IMAGES} with GPS Log directory {GPS_DIRECTORY} with {MAA_MATCHING_SECONDS} maximum seconds gap between image and nearest GPS reading timestamps."
            , imageDirectory, gpsLogDirectory,  maxMatchingSeconds);

        try
        {
            var tagImageResult = await _photoTagger.TagImagesAsync(imageDirectory, gpsLogDirectory, maxMatchingSeconds);
            _logger.LogInformation("Completed tagging {TAGGED_COUNT} out of {TOTAL_COUNT} images. Skipped {SKIP_COUNT}",
                tagImageResult.TaggedImages, tagImageResult.TotalImages, tagImageResult.SkippedImages);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tagging images.");
            return 1;
        }
    }

}