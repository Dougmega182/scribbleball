using FastBoard.Core.Models;

namespace FastBoard.Core.Services;

public sealed class ResizeTokenAction : IUndoableAction
{
    private readonly Token _token;
    private readonly float _before;
    private readonly float _after;
    public string Name => "Resize Token";
    public ResizeTokenAction(Token t, float before, float after) { _token = t; _before = before; _after = after; }
    public void Do() { _token.Scale = _after; }
    public void Undo() { _token.Scale = _before; }
}

public sealed class RotateTokenAction : IUndoableAction
{
    private readonly Token _token;
    private readonly float _before;
    private readonly float _after;
    public string Name => "Rotate Token";
    public RotateTokenAction(Token t, float before, float after) { _token = t; _before = before; _after = after; }
    public void Do() { _token.Rotation = _after; }
    public void Undo() { _token.Rotation = _before; }
}
