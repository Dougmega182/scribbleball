using System.Numerics;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.Core.Services;

public sealed class AddShapeAction : IUndoableAction
{
    private readonly List<Shape> _store;
    private readonly Shape _shape;
    public string Name => "Add Shape";
    public AddShapeAction(List<Shape> store, Shape shape) { _store = store; _shape = shape; }
    public void Do() { if (!_store.Contains(_shape)) _store.Add(_shape); }
    public void Undo() { _store.Remove(_shape); }
}

public sealed class RemoveShapeAction : IUndoableAction
{
    private readonly List<Shape> _store;
    private readonly Shape _shape;
    private int _index = -1;
    public string Name => "Remove Shape";
    public RemoveShapeAction(List<Shape> store, Shape shape) { _store = store; _shape = shape; }
    public void Do()
    {
        _index = _store.IndexOf(_shape);
        if (_index >= 0) _store.RemoveAt(_index);
    }
    public void Undo()
    {
        if (_index < 0) { _store.Add(_shape); return; }
        _index = Math.Clamp(_index, 0, _store.Count);
        _store.Insert(_index, _shape);
    }
}

public sealed class MoveShapeAction : IUndoableAction
{
    private readonly Shape _shape;
    private readonly Vector2 _delta;
    public string Name => "Move Shape";
    public MoveShapeAction(Shape shape, Vector2 delta) { _shape = shape; _delta = delta; }
    public void Do() => Apply(_delta);
    public void Undo() => Apply(-_delta);

    public static void ApplyTo(Shape shape, Vector2 d)
    {
        switch (shape)
        {
            case Token t: t.Position += d; break;
            case ArrowShape a: a.Start += d; a.End += d; break;
            case DashedShape ds:
                for (int i=0;i<ds.Points.Count;i++) ds.Points[i] += d; break;
            case ScreenShape s:
                s.Center += d; break;
            case CurveShape c:
                c.P0 += d; c.P1 += d; c.P2 += d; break;
        }
    }

    private void Apply(Vector2 d) => ApplyTo(_shape, d);
}

// Special action used to commit an already-applied drag. Do() is a no-op, Undo/Redo move shape by +/-delta.
public sealed class DragMoveCommitAction : IUndoableAction
{
    private readonly Shape _shape;
    private readonly Vector2 _delta;
    public string Name => "Drag Move";
    public DragMoveCommitAction(Shape shape, Vector2 delta) { _shape = shape; _delta = delta; }
    public void Do() { /* already applied during drag */ }
    public void Undo() => MoveShapeAction.ApplyTo(_shape, -_delta);
    public void Redo() => MoveShapeAction.ApplyTo(_shape, _delta);
}

// Specific handle edit actions for better undo fidelity
public sealed class EditArrowEndpointAction : IUndoableAction
{
    private readonly ArrowShape _shape;
    private readonly bool _start;
    private readonly Vector2 _before;
    private readonly Vector2 _after;
    public string Name => _start ? "Edit Arrow Start" : "Edit Arrow End";
    public EditArrowEndpointAction(ArrowShape shape, bool start, Vector2 before, Vector2 after)
    { _shape = shape; _start = start; _before = before; _after = after; }
    public void Do() { if (_start) _shape.Start = _after; else _shape.End = _after; }
    public void Undo() { if (_start) _shape.Start = _before; else _shape.End = _before; }
}

public sealed class EditCurvePointAction : IUndoableAction
{
    private readonly CurveShape _shape;
    private readonly int _index; // 0,1,2
    private readonly Vector2 _before;
    private readonly Vector2 _after;
    public string Name => $"Edit Curve P{_index}";
    public EditCurvePointAction(CurveShape shape, int index, Vector2 before, Vector2 after)
    { _shape = shape; _index = index; _before = before; _after = after; }
    public void Do() => Set(_after);
    public void Undo() => Set(_before);
    private void Set(Vector2 v)
    {
        switch (_index)
        {
            case 0: _shape.P0 = v; break; case 1: _shape.P1 = v; break; case 2: _shape.P2 = v; break;
        }
    }
}

public sealed class ResizeScreenAction : IUndoableAction
{
    private readonly ScreenShape _shape;
    private readonly Vector2 _beforeSize;
    private readonly Vector2 _afterSize;
    public string Name => "Resize Screen";
    public ResizeScreenAction(ScreenShape s, Vector2 beforeSize, Vector2 afterSize)
    { _shape = s; _beforeSize = beforeSize; _afterSize = afterSize; }
    public void Do() { _shape.Size = _afterSize; }
    public void Undo() { _shape.Size = _beforeSize; }
}

public sealed class RotateScreenAction : IUndoableAction
{
    private readonly ScreenShape _shape;
    private readonly float _before;
    private readonly float _after;
    public string Name => "Rotate Screen";
    public RotateScreenAction(ScreenShape s, float beforeAngle, float afterAngle)
    { _shape = s; _before = beforeAngle; _after = afterAngle; }
    public void Do() { _shape.Rotation = _after; }
    public void Undo() { _shape.Rotation = _before; }
}
