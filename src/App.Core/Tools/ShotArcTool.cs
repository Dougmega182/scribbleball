using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class ShotArcShape : Shape
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; } = 30f;
    public float StartAngleDeg { get; set; } = 200f;
    public float SweepDeg { get; set; } = 140f;
    public float Thickness { get; set; } = 3f;
}

public class ShotArcTool : IDrawingTool
{
    public ToolType Type => ToolType.ShotArc;
    private readonly List<Shape> _store; private ShotArcShape? _current;
    private readonly Action<Shape>? _onCompleted;
    public ShotArcTool(List<Shape> store, Action<Shape>? onCompleted = null) { _store = store; _onCompleted = onCompleted; }
    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new ShotArcShape{ Center = pt, Radius = 10 + 40*pressure, Thickness = MathF.Max(2f, 6f*pressure) };
    }
    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (_current==null) return; _current.Radius = MathF.Max(5f, (pt - _current.Center).Length());
    }
    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current==null) return; _store.Add(_current); _onCompleted?.Invoke(_current); _current = null;
    }
}
