using System.Numerics;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.Core.Geometry;

public static class HitTest
{
    public enum HandleType { None, ArrowStart, ArrowEnd, CurveP0, CurveP1, CurveP2, ScreenNW, ScreenNE, ScreenSW, ScreenSE, ScreenRotate, TokenResize, TokenRotate }
    public readonly struct HandleHit
    {
        public readonly Shape Shape;
        public readonly HandleType Handle;
        public readonly Vector2 Position;
        public HandleHit(Shape shape, HandleType handle, Vector2 pos) { Shape = shape; Handle = handle; Position = pos; }
        public bool IsValid => Shape != null && Handle != HandleType.None;
    }

    public static bool Hit(Shape s, Vector2 pt, float radius = 10f)
        => s switch
        {
            ArrowShape a => DistancePointToSegment(pt, a.Start, a.End) <= radius,
            DashedShape d => HitDashed(d, pt, radius),
            Token t => (t.Position - pt).Length() <= 24 * t.Scale + radius*0.25f,
            ScreenShape r => HitScreen(r, pt),
            CurveShape c => HitCurve(c, pt, radius),
            ShotArcShape sa => HitShotArc(sa, pt, radius),
            StrokeShape st => HitStroke(st, pt, radius),
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

    public static HandleHit HitHandles(Shape s, Vector2 pt, float r = 10f)
    {
        switch (s)
        {
            case ArrowShape a:
                if ((a.Start - pt).Length() <= r) return new HandleHit(s, HandleType.ArrowStart, a.Start);
                if ((a.End - pt).Length() <= r) return new HandleHit(s, HandleType.ArrowEnd, a.End);
                break;
            case CurveShape c:
                if ((c.P0 - pt).Length() <= r) return new HandleHit(s, HandleType.CurveP0, c.P0);
                if ((c.P1 - pt).Length() <= r) return new HandleHit(s, HandleType.CurveP1, c.P1);
                if ((c.P2 - pt).Length() <= r) return new HandleHit(s, HandleType.CurveP2, c.P2);
                break;
            case ScreenShape scr:
                var half = scr.Size/2;
                var nw = scr.Center - half; var se = scr.Center + half; var ne = new Vector2(se.X, nw.Y); var sw = new Vector2(nw.X, se.Y);
                if ((nw - pt).Length() <= r) return new HandleHit(s, HandleType.ScreenNW, nw);
                if ((ne - pt).Length() <= r) return new HandleHit(s, HandleType.ScreenNE, ne);
                if ((sw - pt).Length() <= r) return new HandleHit(s, HandleType.ScreenSW, sw);
                if ((se - pt).Length() <= r) return new HandleHit(s, HandleType.ScreenSE, se);
                // simple rotate handle: above center
                var rot = scr.Center + new Vector2(0, -half.Y - 20);
                if ((rot - pt).Length() <= r) return new HandleHit(s, HandleType.ScreenRotate, rot);
                break;
            case Token t:
                // token resize handle at southeast, rotate handle above
                float size = 24 * t.Scale;
                var resize = t.Position + new Vector2(size, size);
                var trot = t.Position + new Vector2(0, -size - 20);
                if ((resize - pt).Length() <= r) return new HandleHit(s, HandleType.TokenResize, resize);
                if ((trot - pt).Length() <= r) return new HandleHit(s, HandleType.TokenRotate, trot);
                break;
        }
        return new HandleHit();
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

    private static bool HitShotArc(ShotArcShape s, Vector2 pt, float r)
    {
        // Check distance from circle perimeter near arc angles
        var v = pt - s.Center;
        float dist = MathF.Abs(v.Length() - s.Radius);
        if (dist > r) return false;
        float ang = MathF.Atan2(v.Y, v.X) * 180f / MathF.PI; if (ang < 0) ang += 360f;
        float start = s.StartAngleDeg % 360f; if (start < 0) start += 360f;
        float end = (start + s.SweepDeg) % 360f; if (end < 0) end += 360f;
        bool within = s.SweepDeg >= 0
            ? (start <= end ? ang >= start && ang <= end : ang >= start || ang <= end)
            : (start >= end ? ang <= start && ang >= end : ang <= start || ang >= end);
        return within;
    }

    private static bool HitStroke(StrokeShape s, Vector2 pt, float r)
    {
        for (int i=1;i<s.Points.Count;i++)
        {
            if (DistancePointToSegment(pt, s.Points[i-1], s.Points[i]) <= r) return true;
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
