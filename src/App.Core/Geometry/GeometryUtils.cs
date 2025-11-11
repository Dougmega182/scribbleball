using System.Numerics;

namespace FastBoard.Core.Geometry;

public static class GeometryUtils
{
    public static double Distance(Vector2 a, Vector2 b)
        => Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));

    public static (Vector2 p1, Vector2 p2) ArrowHead(Vector2 from, Vector2 to, float headLength, float headAngleDeg)
    {
        var dir = Vector2.Normalize(to - from);
        float angle = MathF.Atan2(dir.Y, dir.X);
        float a1 = angle + DegreesToRadians(headAngleDeg);
        float a2 = angle - DegreesToRadians(headAngleDeg);
        var p1 = to - new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * headLength;
        var p2 = to - new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * headLength;
        return (p1, p2);
    }

    public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        var u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        var u = 1 - t;
        return u*u*u*p0 + 3*u*u*t*p1 + 3*u*t*t*p2 + t*t*t*p3;
    }

    public static float DegreesToRadians(float deg) => (float)(Math.PI / 180.0) * deg;
}
