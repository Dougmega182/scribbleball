using System.Numerics;
using FastBoard.Core.Geometry;
using FastBoard.Core.Models;

namespace FastBoard.Core.Tools;

public class SelectTool : IDrawingTool
{
    public ToolType Type => ToolType.Select;
    private readonly List<Shape> _store;
    private Shape? _selected;
    private Vector2 _last;
    private bool _dragging;

    public SelectTool(List<Shape> shapeStore) => _store = shapeStore;

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _selected = null;
        _last = pt;
        _dragging = false;
        for (int i=_store.Count-1;i>=0;i--)
        {
            if (HitTest.Hit(_store[i], pt)) { _selected = _store[i]; break; }
        }
        foreach (var s in _store) s.Selected = false;
        if (_selected != null) { _selected.Selected = true; _dragging = true; }
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (!_dragging || _selected == null) return;
        var delta = pt - _last; _last = pt;
        switch (_selected)
        {
            case Token t: t.Position += delta; break;
            case ArrowShape a: a.Start += delta; a.End += delta; break;
            case DashedShape d:
                for (int i=0;i<d.Points.Count;i++) d.Points[i] += delta; break;
            case ScreenShape s:
                s.Center += delta; break;
            case CurveShape c:
                c.P0 += delta; c.P1 += delta; c.P2 += delta; break;
        }
    }
    public void End(Vector2 pt, float pressure = 0.5f) { _dragging = false; }
}
