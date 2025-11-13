using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class PenTool : IDrawingTool
{
    public ToolType Type => ToolType.Pen;
    private readonly List<Shape> _store;
    private readonly Action<Shape>? _onCompleted;
    private StrokeShape? _current;

    public PenTool(List<Shape> store, Action<Shape>? onCompleted = null) { _store = store; _onCompleted = onCompleted; }

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new StrokeShape{ ThicknessBase = 2f };
        _current.Points.Add(pt); _current.Pressures.Add(pressure);
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.Points.Add(pt); _current.Pressures.Add(pressure);
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.Points.Add(pt); _current.Pressures.Add(pressure);
        _store.Add(_current);
        _onCompleted?.Invoke(_current);
        _current = null;
    }
}
