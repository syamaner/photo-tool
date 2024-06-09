// ReSharper disable ClassNeverInstantiated.Global
namespace PhotoTool.Core.Gps;

using System.ComponentModel.DataAnnotations;

public class GpsLogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    [Range(-90.0, 90.0)] public decimal Latitude { get; init; }

    [Range(-180.0, 180.0)] public decimal Longitude { get; init; }
    public decimal AltitudeM { get; init; }
    public decimal SpeedKmH { get; init; }
    [Range(0, 360.0)] public decimal Course { get; init; }
    public short VisibleSatellites { get; init; }
    public short SatellitesCNg22 { get; init; }
    public decimal Hdop { get; init; }

    public override int GetHashCode()
    {
        return Timestamp.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GpsLogEntry);
    }

    private bool Equals(GpsLogEntry? other)
    {
        return other != null && Timestamp.Equals(other.Timestamp);
    }
}