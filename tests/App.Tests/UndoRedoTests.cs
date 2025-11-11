using FastBoard.Core.Services;
using Xunit;

namespace App.Tests;

file class Counter
{
    public int Value { get; private set; }
    public void Inc() => Value++;
    public void Dec() => Value--;
}

file class IncAction : IUndoableAction
{
    private readonly Counter _c;
    public string Name => "inc";
    public IncAction(Counter c) => _c = c;
    public void Do() => _c.Inc();
    public void Undo() => _c.Dec();
}

public class UndoRedoTests
{
    [Fact]
    public void UndoRedo_Works()
    {
        var svc = new UndoRedoService();
        var c = new Counter();
        svc.Execute(new IncAction(c));
        svc.Execute(new IncAction(c));
        Assert.Equal(2, c.Value);
        Assert.True(svc.Undo());
        Assert.Equal(1, c.Value);
        Assert.True(svc.Redo());
        Assert.Equal(2, c.Value);
    }
}
