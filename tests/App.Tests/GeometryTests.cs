using Xunit;

namespace App.Tests;

public class GeometryTests
{
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
