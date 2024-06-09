namespace PhotoTool.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoTool.Core.Gps;
using PhotoTool.Core.Gps.IO;
using PhotoTool.Core.Imaging;

public class PhotoTagger(
    IGpsLogReader gpsLogReader,
    ILogger<PhotoTagger> logger,
    IOptions<CliConfigurationOptions> options)
    : IPhotoTagger
{
    private readonly CliConfigurationOptions _options = options.Value;
    private IEnumerable<GpsLogEntry>? _logEntries;

    public async Task<TagImageResult> TagImagesAsync(string imageDirectory, string logDirectory, int deltaSeconds)
    {
        LoadGpsLogs(logDirectory);
        var imageFileNames = GetImageFiles(imageDirectory).ToList();
        var taggedImageCount = 0;
        var totalImageCount = 0;
        var skippedImageCount = 0;
        var untaggedImageCount = 0;

        foreach (var imageFileName in imageFileNames)
        {
            if (imageFileName.Contains("._"))
                continue;

            totalImageCount += 1;

            if (ImageMetadataHelper.GpsTagExists(imageFileName))
            {
                skippedImageCount++;
                continue;
            }

            var imageTimestamp = ImageMetadataHelper.GetImageCaptureTimestamp(imageFileName);
            if (imageTimestamp == null)
            {
                logger.LogWarning("Error. Cannot retrieve timestamp for image {IMAGE}", imageFileName);
                untaggedImageCount++;
                continue;
            }

            var nearestGpsLogEntry = GetNearestGpsLogEntry(imageTimestamp.Value, deltaSeconds);
            if (nearestGpsLogEntry == null)
            {
                logger.LogWarning("Skipping image {IMAGE} as no Gps log has been matched.", imageFileName);
                untaggedImageCount++;
                continue;
            }

            var tagged = await AddTagToImage(imageFileName, nearestGpsLogEntry, imageTimestamp.Value);
            if (tagged)
                taggedImageCount++;
            else
                untaggedImageCount++;
        }

        return new TagImageResult(totalImageCount,  taggedImageCount, skippedImageCount, untaggedImageCount);
    }

    public async Task<TagImageResult> RemoveLocationMetadataFromImages(string imageDirectory)
    {
        var taggedImageCount = 0;
        var totalImageCount = 0;
        var skippedImageCount = 0;
        var untaggedImageCount = 0;
        var imageFileNames = GetImageFiles(imageDirectory);
        
        foreach (var imageFileName in imageFileNames)
        {
            if (imageFileName.Contains("._"))
                continue;

            totalImageCount += 1;
            if (!ImageMetadataHelper.GpsTagExists(imageFileName))
            {
                skippedImageCount++;
                continue;
            }

            var tagRemoved = await ImageMetadataHelper.RemoveTagAsync(imageFileName);
            if (tagRemoved)
            {
                taggedImageCount += 1;
            }
            else
            {
                untaggedImageCount += 1;
            }
        }

        return new TagImageResult(totalImageCount,  taggedImageCount, skippedImageCount, untaggedImageCount);
    }

    private IEnumerable<string> GetImageFiles(string imageDirectory)
    {
        var imageFileNames =
            Directory.GetFiles(imageDirectory, "*.*", SearchOption.AllDirectories)
                .Where(x =>
                    _options.SupportedImageExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase));
        return imageFileNames;
    }

    private async Task<bool> AddTagToImage(string imageFileName, GpsLogEntry nearestGpsLogEntry,
        DateTimeOffset imageTimestamp)
    {
        var imageTaggingResult = await ImageMetadataHelper.TagImageAsync(imageFileName, nearestGpsLogEntry);

        if (imageTaggingResult)
        {
            logger.LogInformation("Tagged image {IMAGE} with {TIME_STAMP} Lat: {LAT}, Lon: {LON}, Alt {Alt}.",
                imageFileName,
                imageTimestamp,
                nearestGpsLogEntry.Latitude,
                nearestGpsLogEntry.Longitude,
                nearestGpsLogEntry.AltitudeM);
            return true;
        }

        logger.LogError("Error tagging image {IMAGE}, message: {MESSAGE}", imageFileName, imageTaggingResult);
        return false;
    }

    private GpsLogEntry? GetNearestGpsLogEntry(DateTimeOffset imageTimestamp, int deltaSeconds = 10)
    {
        GpsLogEntry? logEntry = null;

        var minSecondsDeltaBetweenImageAndLogs = double.MaxValue;
        foreach (var gpsLogEntry in _logEntries!)
        {
            var differenceSeconds = Math.Abs(imageTimestamp.Subtract(gpsLogEntry.Timestamp).TotalSeconds);
            if (differenceSeconds > minSecondsDeltaBetweenImageAndLogs)
                continue;

            minSecondsDeltaBetweenImageAndLogs = differenceSeconds;
            if (minSecondsDeltaBetweenImageAndLogs < deltaSeconds)
            {
                logEntry = gpsLogEntry;
            }
        }

        return logEntry;
    }

    private void LoadGpsLogs(string logDirectory)
    {
        _logEntries = gpsLogReader.ReadGpsLog(logDirectory);
    }
}