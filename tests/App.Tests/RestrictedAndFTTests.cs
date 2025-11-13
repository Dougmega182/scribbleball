using FastBoard.Core.Geometry;
using Xunit;

namespace App.Tests;

public class RestrictedAndFTTests
{
    [Theory]
    [InlineData(96, 1.0, 6.0)]
    [InlineData(120, 1.25, 6.0)]
    public void FreeThrowCircle_Radius_Pixels_Matches(double dpi, double scale, double ftRadiusFt)
    {
        var px = CourtGeometry.FtToPx(ftRadiusFt, dpi, scale);
        // 6 ft should map linearly to pixels
        Assert.Equal(ftRadiusFt * 12.0 * dpi / 96.0 * scale, px, 8);
    }

    [Theory]
    [InlineData(0, 0, 10, 5)]
    [InlineData(2, 1, 5, 3)]
    public void CircleVerticalLineIntersectionY_Works(double cx, double cy, double r, double vx)
    {
        var (y1, y2) = CourtGeometry.CircleVerticalLineIntersectionY(cx, cy, r, vx);
        if (Math.Abs(vx - cx) > r)
        {
            Assert.True(double.IsNaN(y1) && double.IsNaN(y2));
        }
        else
        {
            // Both points lie on the circle
            double dx = vx - cx;
            double dy = y1 - cy;
            Assert.InRange(dx*dx + dy*dy, r*r - 1e-6, r*r + 1e-6);
            dy = y2 - cy;
            Assert.InRange(dx*dx + dy*dy, r*r - 1e-6, r*r + 1e-6);
            Assert.True(y1 <= y2);
        }
    }
}
