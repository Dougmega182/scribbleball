using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class EraserTool : IDrawingTool
{
    public ToolType Type => ToolType.Eraser;
    private readonly List<Shape> _store;
    private readonly float _radius;

    public EraserTool(List<Shape> shapeStore, float radius = 12f)
    {
        _store = shapeStore; _radius = radius;
    }

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        EraseAt(pt);
    }
    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        EraseAt(pt);
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        EraseAt(pt);
    }

    private void EraseAt(Vector2 pt)
    {
        for (int i=_store.Count-1; i>=0; i--)
        {
            if (_store[i] is ArrowShape a)
            {
                if (DistanceToSegment(pt, a.Start, a.End) <= _radius) { _store.RemoveAt(i); break; }
            }
            else if (_store[i] is DashedShape d)
            {
                for (int j=1;j<d.Points.Count;j++)
                {
                    if (DistanceToSegment(pt, d.Points[j-1], d.Points[j]) <= _radius) { _store.RemoveAt(i); break; }
                }
            }
            else if (_store[i] is ScreenShape s)
            {
                var half = s.Size/2;
                if (pt.X >= s.Center.X-half.X && pt.X <= s.Center.X+half.X && pt.Y >= s.Center.Y-half.Y && pt.Y <= s.Center.Y+half.Y) { _store.RemoveAt(i); break; }
            }
        }
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a; var ap = p - a;
        float t = Vector2.Dot(ap, ab) / MathF.Max(1e-5f, Vector2.Dot(ab, ab));
        t = MathF.Min(1, MathF.Max(0, t));
        var proj = a + t*ab;
        return (proj - p).Length();
    }
}
