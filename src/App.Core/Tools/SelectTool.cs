using System.Numerics;
using FastBoard.Core.Geometry;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class SelectTool : IDrawingTool
{
    public ToolType Type => ToolType.Select;
    private readonly List<Shape> _store;
    private Shape? _selected;

    public SelectTool(List<Shape> shapeStore) => _store = shapeStore;

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _selected = null;
        for (int i=_store.Count-1;i>=0;i--)
        {
            if (HitTest.Hit(_store[i], pt)) { _selected = _store[i]; break; }
        }
        foreach (var s in _store) s.Selected = false;
        if (_selected != null) _selected.Selected = true;
    }

    public void Move(Vector2 pt, float pressure = 0.5f) { }
    public void End(Vector2 pt, float pressure = 0.5f) { }
}
