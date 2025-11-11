using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class ArrowTool : IDrawingTool
{
    public ToolType Type => ToolType.Arrow;
    private readonly List<Shape> _store;
    private ArrowShape? _current;

    public ArrowTool(List<Shape> shapeStore) => _store = shapeStore;

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new ArrowShape { Start = pt, End = pt, Thickness = MathF.Max(2f, 8f*pressure) };
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.End = pt;
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.End = pt;
        _store.Add(_current);
        _current = null;
    }
}
