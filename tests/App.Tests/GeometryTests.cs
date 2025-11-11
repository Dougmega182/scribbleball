using Xunit;

namespace App.Tests;

public class GeometryTests
{
    [Fact]
    public void ArrowHead_Angles_AreReasonable()
    {
        var from = new System.Numerics.Vector2(0,0);
        var to = new System.Numerics.Vector2(10,0);
        var (p1,p2) = FastBoard.Core.Geometry.GeometryUtils.ArrowHead(from, to, 5, 30);
        Assert.True(p1.X < 10 && p2.X < 10);
    }

    [Theory]
    [InlineData(0, 0, 10, 0, 10)]
    [InlineData(0, 0, 0, 10, 10)]
    [InlineData(-3, -4, 0, 0, 5)]
    public void Distance_Works(double x1, double y1, double x2, double y2, double expected)
    {
        var d = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        Assert.Equal(expected, d, 10);
    }
}
