namespace PhotoTool.Tests;

using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoTool.Core;
using PhotoTool.Core.Gps.IO;
using PhotoTool.Core.Imaging;

public class PhotoTaggerTests: IAsyncLifetime
{
    private const string TestDirectory = "testData";
    private const string ImageSourceDirectory = "images";
    private const string LogSourceDirectory = "logs";
    private readonly ITestLoggerFactory _loggerFactory = TestLoggerFactory.Create();
    
    [Fact]
    public async Task GpsTagExists_ReturnsReturnsCorrectResults()
    {
        var logger = _loggerFactory.CreateLogger<PhotoTagger>();
        var imageDirectory = Path.Combine(TestDirectory, ImageSourceDirectory);
        var logDirectory = Path.Combine(TestDirectory, LogSourceDirectory);
        const int deltaSeconds = 600;
        var options = Options.Create(new CliConfigurationOptions()
        {
            SupportedImageExtensions = [".jpg"]
        });
        
        var photoTagger  = new PhotoTagger(new CsvGpsLogReader(), logger, options);
        var result = await photoTagger.TagImagesAsync(imageDirectory, logDirectory, deltaSeconds);
        
        result.Should().NotBeNull();
        result.TaggedImages.Should().Be(4);
        result.UntaggedImageCount.Should().Be(6);
        result.TotalImages.Should().Be(10);
        result.SkippedImages.Should().Be(0);
        
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(TestDirectory);
        Directory.CreateDirectory(Path.Combine(TestDirectory, ImageSourceDirectory));
        Directory.CreateDirectory(Path.Combine(TestDirectory, LogSourceDirectory));
       
        foreach (var file in Directory.GetFiles(ImageSourceDirectory))
        {
            var destinationFile = Path.Combine(TestDirectory, ImageSourceDirectory, Path.GetFileName(file));
            // delete if destination file exists
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }
            
            File.Copy(file, destinationFile);
            await ImageMetadataHelper.RemoveTagAsync(destinationFile);
        }
        
        foreach (var file in Directory.GetFiles(LogSourceDirectory))
        {
            var destinationFile = Path.Combine(TestDirectory, LogSourceDirectory, Path.GetFileName(file));
            // delete if destination file exists
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }
            File.Copy(file, destinationFile);
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}