using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class DribbleTool : IDrawingTool
{
    public ToolType Type => ToolType.Dribble;
    private readonly List<Shape> _store;
    private DashedShape? _current;

    public DribbleTool(List<Shape> shapeStore) => _store = shapeStore;

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _current = new DashedShape { Points = new(){ pt }, Thickness = MathF.Max(2f, 6f*pressure) };
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        _current?.Points.Add(pt);
    }

    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_current == null) return;
        _current.Points.Add(pt);
        _store.Add(_current);
        _current = null;
    }
}
