namespace PhotoTool.Core;

public record TagImageResult(int TotalImages, int TaggedImages, int SkippedImages, int UntaggedImageCount);

public interface IPhotoTagger
{
   Task<TagImageResult> TagImagesAsync(string imageDirectory, string logDirectory, int deltaSeconds);
   Task<TagImageResult> RemoveLocationMetadataFromImages(string imageDirectory);
}