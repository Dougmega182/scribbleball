using System.Numerics;
using FastBoard.Core.Geometry;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;
using Xunit;

namespace App.Tests;

public class HitTestTests
{
    [Fact]
    public void Arrow_Hit_Segment()
    {
        var a = new ArrowShape{ Start=new(0,0), End=new(10,0)};
        Assert.True(HitTest.Hit(a, new Vector2(5,1), 2));
        Assert.False(HitTest.Hit(a, new Vector2(5,5), 1));
    }
}
