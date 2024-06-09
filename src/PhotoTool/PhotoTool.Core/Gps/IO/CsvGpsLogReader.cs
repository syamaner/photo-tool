namespace PhotoTool.Core.Gps.IO;

using System.Globalization;
using CsvHelper;

/// <summary>
/// Currently only supports CSV logs from an iGotu GPS logger.
/// </summary>
public class CsvGpsLogReader: IGpsLogReader
{
    private const string FileWildcard = "*.csv";
    
    public IEnumerable<GpsLogEntry> ReadGpsLog(string directory)
    {
        var logFiles = Directory.GetFiles(directory, FileWildcard);
        var  gpsLogs = new HashSet<GpsLogEntry>();

        foreach (var file in logFiles)
        {
            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<LogEntryMap>();
            var records = csv.GetRecords<GpsLogEntry>();
            foreach (var record in records)
            {
                gpsLogs.Add(record);
            }
        }

        return gpsLogs;
    }
}