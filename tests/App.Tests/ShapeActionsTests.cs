using System.Numerics;
using FastBoard.Core.Models;
using FastBoard.Core.Services;
using Xunit;

namespace App.Tests;

public class ShapeActionsTests
{
    [Fact]
    public void AddRemove_Move_UndoRedo_Works()
    {
        var list = new List<Shape>();
        var undo = new UndoRedoService();
        var arrow = new ArrowShape{ Start = new(0,0), End = new(10,0) };
        undo.Execute(new AddShapeAction(list, arrow));
        Assert.Single(list);
        undo.Execute(new MoveShapeAction(arrow, new Vector2(5, 2)));
        Assert.Equal(new Vector2(5,2), arrow.Start);
        Assert.Equal(new Vector2(15,2), arrow.End);
        Assert.True(undo.Undo()); // undo move
        Assert.Equal(new Vector2(0,0), arrow.Start);
        Assert.Equal(new Vector2(10,0), arrow.End);
        Assert.True(undo.Undo()); // undo add
        Assert.Empty(list);
        Assert.True(undo.Redo()); // redo add
        Assert.Single(list);
        Assert.True(undo.Redo()); // redo move
        Assert.Equal(new Vector2(5,2), arrow.Start);
        Assert.Equal(new Vector2(15,2), arrow.End);
        undo.Execute(new RemoveShapeAction(list, arrow));
        Assert.Empty(list);
        Assert.True(undo.Undo()); // undo remove
        Assert.Single(list);
    }
}
