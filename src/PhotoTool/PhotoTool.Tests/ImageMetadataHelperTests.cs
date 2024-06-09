namespace PhotoTool.Tests;

using System.Globalization;
using AutoFixture.Xunit2;
using FluentAssertions;
using PhotoTool.Core.Gps;
using PhotoTool.Core.Imaging;
using Xunit.Abstractions;

public class ImageMetadataHelperTests(ITestOutputHelper testOutputHelper)
{
    private const string DateTimeOffsetParseFormat = "dd/MM/yyyy HH:mm:ss zzz";
    private static readonly CultureInfo Provider = CultureInfo.InvariantCulture;

    [Theory]
    [InlineData("images/british_library_match.jpg", true)]
    [InlineData("images/no_match.jpg", false)]
    [InlineData("images/DSC_0166.jpg", true)]
    public void GpsTagExists_ReturnsReturnsCorrectResults(string path, bool tagExists)
    {
        var result = ImageMetadataHelper.GpsTagExists(path);

        result.Should().Be(tagExists);
    }
    
    [Theory]
    [InlineData("images/british_library_match.jpg", "20/05/2024 15:11:25 +01:00")]
    [InlineData("images/no_match.jpg", "20/05/2024 11:51:37 +01:00")]
    [InlineData("images/royal_albert_hall_extended_match_1.jpg", "20/05/2024 21:32:59 +01:00")]
    public void GetImageCaptureTimestamp_ReturnsExpectedCaptureTimestamp(string path, string timestamp)
    {
        var maximumAllowedSecondsGap = new TimeSpan(0, 0, 0,0,50);
        var expectedTimeStamp = DateTimeOffset.ParseExact(timestamp, DateTimeOffsetParseFormat, Provider);

        var imageCaptureTimestamp = ImageMetadataHelper.GetImageCaptureTimestamp(path);
        
        imageCaptureTimestamp.Should().BeCloseTo(expectedTimeStamp, maximumAllowedSecondsGap);
    }
 
    [Theory]
    [InlineAutoData("images/british_library_match.jpg")]
    [InlineAutoData("images/no_match.jpg")]
    [InlineAutoData("images/royal_albert_hall_extended_match_1.jpg")]
    public async Task TagImageAsync_TagsTheImageWhenImageDoesNotHaveExistingTags(string path, GpsLogEntry logEntry)
    {
        var result = await ImageMetadataHelper.TagImageAsync(path, logEntry);

        var isTagged = ImageMetadataHelper.GpsTagExists(path);
        
        result.Should().Be(true);
        isTagged.Should().Be(true);
    }
    
    [Theory]
    [InlineAutoData("images/british_library_match.jpg")]
    [InlineAutoData("images/no_match.jpg")]
    [InlineAutoData("images/royal_albert_hall_extended_match_1.jpg")]
    public async Task RemoveTagAsync_RemovedTheGpsTags(string path,GpsLogEntry logEntry)
    {
        await ImageMetadataHelper.TagImageAsync(path, logEntry);
        var tagRemoved = await ImageMetadataHelper.RemoveTagAsync(path);
        
        tagRemoved.Should().Be(true);
        ImageMetadataHelper.GpsTagExists(path).Should().Be(false);
    }
}