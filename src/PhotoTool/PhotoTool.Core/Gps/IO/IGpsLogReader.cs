namespace PhotoTool.Core.Gps.IO;

public interface IGpsLogReader
{
    IEnumerable<GpsLogEntry>? ReadGpsLog(string directory);
}