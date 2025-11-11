namespace FastBoard.Core.Services;

public interface IUndoableAction { void Do(); void Undo(); string Name { get; } }

public class UndoRedoService
{
    private readonly Stack<IUndoableAction> _undo = new();
    private readonly Stack<IUndoableAction> _redo = new();

    public int UndoCount => _undo.Count;
    public int RedoCount => _redo.Count;

    public void Execute(IUndoableAction action)
    {
        action.Do();
        _undo.Push(action);
        _redo.Clear();
    }

    public bool Undo()
    {
        if (_undo.Count == 0) return false;
        var a = _undo.Pop();
        a.Undo();
        _redo.Push(a);
        return true;
    }

    public bool Redo()
    {
        if (_redo.Count == 0) return false;
        var a = _redo.Pop();
        a.Do();
        _undo.Push(a);
        return true;
    }
}
