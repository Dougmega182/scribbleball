using System.Numerics;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.Core.Geometry;

public static class HitTest
{
    public static bool Hit(Shape s, Vector2 pt, float radius = 10f)
        => s switch
        {
            ArrowShape a => DistancePointToSegment(pt, a.Start, a.End) <= radius,
            DashedShape d => HitDashed(d, pt, radius),
            Token t => (t.Position - pt).Length() <= 24 * t.Scale + radius*0.25f,
            ScreenShape r => HitScreen(r, pt),
            CurveShape c => HitCurve(c, pt, radius),
            _ => false
        };

    private static bool HitDashed(DashedShape d, Vector2 pt, float r)
    {
        for (int i=1;i<d.Points.Count;i++)
            if (DistancePointToSegment(pt, d.Points[i-1], d.Points[i]) <= r) return true;
        return false;
    }

    private static bool HitScreen(ScreenShape s, Vector2 pt)
    {
        var half = s.Size/2;
        return pt.X >= s.Center.X-half.X && pt.X <= s.Center.X+half.X && pt.Y >= s.Center.Y-half.Y && pt.Y <= s.Center.Y+half.Y;
    }

    private static bool HitCurve(CurveShape c, Vector2 pt, float r)
    {
        const int samples = 32;
        var prev = c.P0;
        for (int i=1;i<=samples;i++)
        {
            float t = i/(float)samples;
            var cur = GeometryUtils.QuadraticBezier(c.P0, c.P1, c.P2, t);
            if (DistancePointToSegment(pt, prev, cur) <= r) return true;
            prev = cur;
        }
        return false;
    }

    public static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a; var ap = p - a;
        float t = Vector2.Dot(ap, ab) / MathF.Max(1e-5f, Vector2.Dot(ab, ab));
        t = MathF.Min(1, MathF.Max(0, t));
        var proj = a + t*ab;
        return (proj - p).Length();
    }
}
