// ReSharper disable ClassNeverInstantiated.Global
namespace PhotoTool.Core.Gps.IO;

using CsvHelper.Configuration;

internal sealed class LogEntryMap : ClassMap<GpsLogEntry>
{
    public LogEntryMap()
    {
        Map(m => m.Latitude).Name("Latitude");
        Map(m => m.Longitude).Name("Longitude");
        Map(m => m.AltitudeM).Name("Altitude(m)");
        Map(m => m.SpeedKmH).Name("Speed(km/h)");
        Map(m => m.Course).Name("Course");
        Map(m => m.VisibleSatellites).Name("Visible Satellites");
        Map(m => m.SatellitesCNg22).Name("Satellites(CN>22)");
        Map(m => m.Hdop).Name("HDOP");

        Map(m => m.Timestamp).Convert(args =>
            DateTimeOffset.Parse(args.Row.GetField("Time").Replace("\"", "").Replace("=", ""))
        );
    }
}