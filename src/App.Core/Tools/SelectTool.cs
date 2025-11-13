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
    private Vector2 _totalDelta;
    private FastBoard.Core.Services.UndoRedoService? _undo;
    private FastBoard.Core.Geometry.HitTest.HandleHit _activeHandle;
    private float _tokenScaleBefore;
    private float _tokenRotationBefore;

    public SelectTool(List<Shape> shapeStore) => _store = shapeStore;

    private static void ApplyHandleDrag(Shape shape, FastBoard.Core.Geometry.HitTest.HandleType handle, Vector2 delta)
    {
        switch (shape)
        {
            case ArrowShape a:
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ArrowStart) a.Start += delta;
                else if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ArrowEnd) a.End += delta;
                break;
            case CurveShape c:
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP0) c.P0 += delta;
                else if (handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP1) c.P1 += delta;
                else if (handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP2) c.P2 += delta;
                break;
            case ScreenShape s:
                var half = s.Size/2;
                // Resize by moving corners relative to center
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenNW) { half -= new Vector2(delta.X/2, delta.Y/2); }
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenNE) { half = new Vector2(half.X + delta.X/2, half.Y - delta.Y/2); }
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenSW) { half = new Vector2(half.X - delta.X/2, half.Y + delta.Y/2); }
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenSE) { half += new Vector2(delta.X/2, delta.Y/2); }
                half = new Vector2(MathF.Max(4, MathF.Abs(half.X)), MathF.Max(4, MathF.Abs(half.Y)));
                s.Size = half*2;
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenRotate) s.Rotation += delta.X; // crude rotation by horizontal drag
                break;
            case Token t:
                if (handle == FastBoard.Core.Geometry.HitTest.HandleType.TokenResize)
                {
                    float before = t.Scale;
                    float size = MathF.Max(0.2f, before + (delta.X + delta.Y) * 0.01f);
                    t.Scale = size;
                }
                else if (handle == FastBoard.Core.Geometry.HitTest.HandleType.TokenRotate)
                {
                    t.Rotation += delta.X; // crude horizontal drag to rotate
                }
                break;
        }
    }

    public void Begin(Vector2 pt, float pressure = 0.5f)
    {
        _selected = null;
        _last = pt;
        _totalDelta = Vector2.Zero;
        _dragging = false;
        // prefer handle hit
        for (int i=_store.Count-1;i>=0;i--)
        {
            var h = HitTest.HitHandles(_store[i], pt);
            if (h.IsValid) { _selected = _store[i]; _activeHandle = h; 
                if (_selected is Token tt) { _tokenScaleBefore = tt.Scale; _tokenRotationBefore = tt.Rotation; }
                break; }
            if (_selected == null && HitTest.Hit(_store[i], pt)) { _selected = _store[i]; }
        }
        foreach (var s in _store) s.Selected = false;
        if (_selected != null) { _selected.Selected = true; _dragging = true; }
    }

    public void Move(Vector2 pt, float pressure = 0.5f)
    {
        if (!_dragging || _selected == null) return;
        var delta = pt - _last; _last = pt;
        _totalDelta += delta;
        if (_activeHandle.IsValid)
        {
            ApplyHandleDrag(_selected, _activeHandle.Handle, delta);
        }
        else
        {
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
    }
    public void End(Vector2 pt, float pressure = 0.5f)
    {
        if (_dragging && _selected != null && _undo != null)
        {
            if (_activeHandle.IsValid)
            {
                CommitHandleEdit(_selected, _activeHandle);
            }
            else if (_totalDelta != Vector2.Zero)
            {
                _undo.Execute(new FastBoard.Core.Services.DragMoveCommitAction(_selected, _totalDelta));
            }
        }
        _activeHandle = default;
        _dragging = false;
    }

    private void CommitHandleEdit(Shape shape, FastBoard.Core.Geometry.HitTest.HandleHit h)
    {
        switch (shape)
        {
            case ArrowShape a:
                if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.ArrowStart)
                    _undo!.Execute(new FastBoard.Core.Services.EditArrowEndpointAction(a, true, a.Start - _totalDelta, a.Start));
                else if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.ArrowEnd)
                    _undo!.Execute(new FastBoard.Core.Services.EditArrowEndpointAction(a, false, a.End - _totalDelta, a.End));
                break;
            case CurveShape c:
                if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP0)
                    _undo!.Execute(new FastBoard.Core.Services.EditCurvePointAction(c, 0, c.P0 - _totalDelta, c.P0));
                else if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP1)
                    _undo!.Execute(new FastBoard.Core.Services.EditCurvePointAction(c, 1, c.P1 - _totalDelta, c.P1));
                else if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.CurveP2)
                    _undo!.Execute(new FastBoard.Core.Services.EditCurvePointAction(c, 2, c.P2 - _totalDelta, c.P2));
                break;
            case ScreenShape s:
                if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.ScreenRotate)
                    _undo!.Execute(new FastBoard.Core.Services.RotateScreenAction(s, s.Rotation - _totalDelta.X, s.Rotation));
                else
                {
                    var before = s.Size - new Vector2(MathF.Sign(_totalDelta.X)*MathF.Abs(_totalDelta.X), MathF.Sign(_totalDelta.Y)*MathF.Abs(_totalDelta.Y));
                    var after = s.Size;
                    _undo!.Execute(new FastBoard.Core.Services.ResizeScreenAction(s, before, after));
                }
                break;
            case Token t:
                if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.TokenResize)
                    _undo!.Execute(new FastBoard.Core.Services.ResizeTokenAction(t, _tokenScaleBefore, t.Scale));
                else if (h.Handle == FastBoard.Core.Geometry.HitTest.HandleType.TokenRotate)
                    _undo!.Execute(new FastBoard.Core.Services.RotateTokenAction(t, _tokenRotationBefore, t.Rotation));
                break;
        }
    }
}
