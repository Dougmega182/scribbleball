namespace FastBoard.Core.Geometry;

public static class CourtGeometry
{
    public enum CourtMode { Half, Full }

    // Dimensions in feet for NBA court; scalable.
    public const double CourtWidthFt = 50.0;   // sideline to sideline
    public const double CourtLengthFt = 94.0;  // baseline to baseline (full), half is 47
    public const double ThreePointRadiusFt = 23.75; // from basket center
    public const double CornerThreeDistFt = 22.0;   // straight 3PT line is at ±22 ft from center (parallel to sidelines)
    public const double HoopHeightFt = 10.0; // not used for 2D
    public const double BasketToBaselineFt = 4.0; // approx: hoop center from baseline
    public const double FreeThrowLineDistFt = 15.0; // from backboard plane
    public const double LaneWidthFt = 16.0;
    public const double RestrictedAreaRadiusFt = 4.0;
    public const double CenterCircleRadiusFt = 6.0;

    public static double FtToPx(double feet, double dpi, double scale)
        => feet * 12.0 * dpi / 96.0 * scale;

    public static (double widthPx, double lengthPx) GetCourtSizePx(bool halfCourt, double dpi, double scale)
    {
        var length = halfCourt ? CourtLengthFt / 2.0 : CourtLengthFt;
        return (FtToPx(CourtWidthFt, dpi, scale), FtToPx(length, dpi, scale));
    }

    public static float GetDpi(WindowingDisplayInfo? info)
        => info?.Dpi ?? 96f;

    public sealed class WindowingDisplayInfo
    {
        public float Dpi { get; init; } = 96f;
    }

    public static double ThreePointArcRadiusPx(double dpi, double scale)
        => FtToPx(ThreePointRadiusFt, dpi, scale);

    public static double RestrictedArcRadiusPx(double dpi, double scale)
        => FtToPx(RestrictedAreaRadiusFt, dpi, scale);

    public static double CenterCircleRadiusPx(double dpi, double scale)
        => FtToPx(CenterCircleRadiusFt, dpi, scale);

    // Compute intersections of circle (cx,cy,r) with a vertical line x = vx.
    // Returns (y1,y2) where y1 <= y2; if no intersection, returns (double.NaN, double.NaN).
    public static (double y1, double y2) CircleVerticalLineIntersectionY(double cx, double cy, double r, double vx)
    {
        double dx = vx - cx;
        double inside = r * r - dx * dx;
        if (inside < 0) return (double.NaN, double.NaN);
        double root = Math.Sqrt(Math.Max(0, inside));
        double y1 = cy - root;
        double y2 = cy + root;
        return (y1, y2);
    }
}
