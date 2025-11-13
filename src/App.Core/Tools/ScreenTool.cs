using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class ScreenShape : Shape
{
    public Vector2 Center { get; set; }
    public Vector2 Size { get; set; } = new(40,20);
    public float CornerRadius { get; set; } = 8f;
    public float Rotation { get; set; }
    public float Thickness { get; set; } = 3f;
}

public class ScreenTool : IDrawingTool
{
    public ToolType Type => ToolType.Screen;
    private readonly List<Shape> _store;
    private readonly Action<Shape>? _onCompleted;
    private ScreenShape? _current;

    public ScreenTool(List<Shape> shapeStore, Action<Shape>? onCompleted = null) { _store = shapeStore; _onCompleted = onCompleted; }

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new ScreenShape { Center = pt, Thickness = MathF.Max(2f, 6f*pressure) };
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.Size = new Vector2(MathF.Abs(pt.X - _current.Center.X)*2, MathF.Abs(pt.Y - _current.Center.Y)*2);
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        Move(pt, pressure);
        _store.Add(_current);
        _onCompleted?.Invoke(_current);
        _current = null;
    }
}
