namespace PhotoTool.Core.Imaging;

using System.Globalization;
using Cysharp.Diagnostics;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoTool.Core.Gps;

public static class ImageMetadataHelper
{
    private static readonly CultureInfo Provider = CultureInfo.InvariantCulture;

    private static readonly HashSet<string> TagsOfInterest =
    [
        ExifConstants.DateTimeExifTagName,
        ExifConstants.UtcOffsetHoursExifTagName
    ];

    public static async Task<bool> ExifToolInstalled()
    {
        var result = await ProcessX
            .StartAsync("exiftool --help")
            .FirstOrDefaultAsync();

        return result != string.Empty;
    }
    
    public static async Task<bool> TagImageAsync(string imageFileName, GpsLogEntry entry)
    {
        var imageTaggingResult = await ProcessX
            .StartAsync($"exiftool -GPSLatitude*={entry.Latitude} -GPSLongitude*={entry.Longitude} -GPSAltitude*={entry.AltitudeM} {imageFileName}")
            .FirstAsync();

        return !string.IsNullOrWhiteSpace(imageTaggingResult) &&
               imageTaggingResult.Contains("image files updated", StringComparison.OrdinalIgnoreCase);
    }
    
    public static async Task<bool> RemoveTagAsync(string imageFileName)
    {
        var imageTaggingResult = await ProcessX
            .StartAsync($"exiftool  \"-gps*=\" {imageFileName}")
            .FirstAsync();

        return !string.IsNullOrWhiteSpace(imageTaggingResult) &&
               imageTaggingResult.Contains("updated", StringComparison.OrdinalIgnoreCase);
    }
    
    public static bool GpsTagExists(string imageFilePath)
    {
        var gps = ImageMetadataReader.ReadMetadata(imageFilePath)
            .OfType<GpsDirectory>()
            .FirstOrDefault();

        return gps is { TagCount: > 0 };
    }

    public static DateTimeOffset? GetImageCaptureTimestamp(string imageFilePath)
    {
        try
        {
            IEnumerable<MetadataExtractor.Directory> metadataDirectories = ImageMetadataReader.ReadMetadata(imageFilePath);
            
            var tagDictionary = metadataDirectories.SelectMany(x => x.Tags)
                .Where(x => TagsOfInterest.Contains(x.Name) )
                .ToDictionary(x => x.Name, x => x.Description);

            if (!tagDictionary.ContainsKey(ExifConstants.DateTimeExifTagName) ||
                string.IsNullOrWhiteSpace(tagDictionary[ExifConstants.DateTimeExifTagName]))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(tagDictionary[ExifConstants.UtcOffsetHoursExifTagName])
                || !tagDictionary.TryGetValue(ExifConstants.UtcOffsetHoursExifTagName, out var value))
            { 
                value = "+00:00";
            } 
            
            var originalDateTime = tagDictionary[ExifConstants.DateTimeExifTagName];
            var timeZone = value;

            return DateTimeOffset.ParseExact(originalDateTime + " " + timeZone, ExifConstants.DateTimeOffsetParseFormat, Provider);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }
}

    
