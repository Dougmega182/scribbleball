using System.Numerics;
using FastBoard.Core.Geometry;
using Xunit;

namespace App.Tests;

public class BezierTests
{
    [Fact]
    public void QuadraticBezier_Endpoints()
    {
        var p0 = new Vector2(0,0); var p1 = new Vector2(10,10); var p2 = new Vector2(20,0);
        Assert.Equal(p0, GeometryUtils.QuadraticBezier(p0,p1,p2,0));
        Assert.Equal(p2, GeometryUtils.QuadraticBezier(p0,p1,p2,1));
    }

    [Fact]
    public void CubicBezier_Endpoints()
    {
        var p0 = new Vector2(0,0); var p1 = new Vector2(5,10); var p2 = new Vector2(15,10); var p3 = new Vector2(20,0);
        Assert.Equal(p0, GeometryUtils.CubicBezier(p0,p1,p2,p3,0));
        Assert.Equal(p3, GeometryUtils.CubicBezier(p0,p1,p2,p3,1));
    }
}
