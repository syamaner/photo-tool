namespace PhotoTool.Commands;

using System.CommandLine;
using PhotoTool.Core.Imaging;

public abstract class MetadataCommand(string name, string? description = null)
    : Command(name, description)
{
    protected static async Task<bool> IsExifToolInstalled()
    {
        try
        {
            return await ImageMetadataHelper.ExifToolInstalled();
        }
        catch (Exception)
        {
            return false;
        }
    }
}