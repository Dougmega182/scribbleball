using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class CurveShape : Shape
{
    public Vector2 P0 { get; set; }
    public Vector2 P1 { get; set; }
    public Vector2 P2 { get; set; }
    public float Thickness { get; set; } = 3f;
}

public class CurveTool : IDrawingTool
{
    public ToolType Type => ToolType.Curve;
    private readonly List<Shape> _store;
    private CurveShape? _current;

    public CurveTool(List<Shape> shapeStore) => _store = shapeStore;

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new CurveShape { P0=pt, P1=pt, P2=pt, Thickness = MathF.Max(2f, 6f*pressure) };
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.P2 = pt;
        _current.P1 = ( _current.P0 + _current.P2 )/2;
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.P2 = pt;
        _store.Add(_current);
        _current = null;
    }
}
