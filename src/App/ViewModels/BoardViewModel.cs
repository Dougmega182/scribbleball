using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;
using FastBoard.Core.Services;

namespace FastBoard.ViewModels;

public partial class BoardViewModel : ObservableObject
{
    public List<Shape> Shapes { get; } = new();

    public UndoRedoService Undo { get; } = new();

    public int UndoCount => Undo.UndoCount;
    public int RedoCount => Undo.RedoCount;
    public string ActiveToolName => ActiveTool.ToString();

    public void NotifyUndoCountsChanged()
    {
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));
    }

    public void TrackAddShape(Shape s)
    {
        Undo.Execute(new AddShapeAction(Shapes, s));
        NotifyUndoCountsChanged();
    }

    public List<FastBoard.Core.Playback.FrameState> Frames { get; } = new();
    public List<double> FrameDurations { get; } = new(); // seconds per segment (Frames[i] -> Frames[i+1])
    [ObservableProperty]
    private int currentFrameIndex = -1;

    public void CaptureFrame()
    {
        Frames.Add(FastBoard.Core.Playback.FrameState.Capture(Shapes));
        // If we now have at least two frames, add a default duration for the new segment
        if (Frames.Count >= 2) FrameDurations.Add(1.0);
        CurrentFrameIndex = Math.Max(0, Frames.Count - 2); // select the new segment start
    }

    public IEnumerable<Shape> GetFrameShapes(float t)
    {
        if (Frames.Count == 0) return Shapes;
        if (CurrentFrameIndex < 0 || CurrentFrameIndex >= Frames.Count-1) return Frames.Last().Shapes;
        var a = Frames[CurrentFrameIndex];
        var b = Frames[CurrentFrameIndex+1];
        var lerped = new List<Shape>();
        var count = Math.Min(a.Shapes.Count, b.Shapes.Count);
        for (int i=0;i<count;i++)
        {
            var sa = a.Shapes[i];
            var sb = b.Shapes[i];
            var tt = FastBoard.Core.Playback.Tween.ApplyEase(t, sa.Easing);
            var s = FastBoard.Core.Playback.Tween.LerpShape(sa, sb, tt);
            lerped.Add(s);
        }
        return lerped;
    }
    public ToolType ActiveTool { get => activeTool; set => SetProperty(ref activeTool, value); }
    private ToolType activeTool = ToolType.None;

    public void DeleteSelected()
    {
        for (int i = Shapes.Count - 1; i >= 0; i--)
        {
            if (Shapes[i].Selected)
            {
                Undo.Execute(new FastBoard.Core.Services.RemoveShapeAction(Shapes, Shapes[i]));
            }
        }
    }

    public void NudgeSelected(int dx, int dy)
    {
        var d = new Vector2(dx, dy);
        foreach (var s in Shapes)
        {
            if (!s.Selected) continue;
            Undo.Execute(new FastBoard.Core.Services.MoveShapeAction(s, d));
        }
    }

    public FastBoard.Core.Models.Token? GetSelectedToken() => Shapes.OfType<FastBoard.Core.Models.Token>().FirstOrDefault(t => t.Selected);

    public void AddToken(string? imagePath, System.Numerics.Vector2 position)
    {
        var t = new FastBoard.Core.Models.Token{ ImagePath = imagePath, Position = position };
        Undo.Execute(new AddShapeAction(Shapes, t));
        NotifyUndoCountsChanged();
    }

    private SelectTool? select;
    private ArrowTool? arrow; private ArrowTool? Arrow => arrow ??= new ArrowTool(Shapes, onCompleted: s => Undo.Execute(new AddShapeAction(Shapes, s)));
    private DribbleTool? dribble;
    private CurveTool? curve;
    private ScreenTool? screen;
    private EraserTool? eraser;
    private ShotArcTool? shot;
    private PenTool? pen;

    public IDrawingTool? GetActiveDrawingTool()
    {
        // Ensure select tool has undo reference for aggregated drag undo
        if (select != null)
        {
            var field = typeof(FastBoard.Core.Tools.SelectTool).GetField("_undo", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(select, Undo);
        }
        return ActiveTool switch
        {
            ToolType.Select => select ??= new SelectTool(Shapes) { }, // will set undo below

            ToolType.Arrow => Arrow,
            ToolType.Dribble => dribble ??= new DribbleTool(Shapes, TrackAddShape),
            ToolType.Curve => curve ??= new CurveTool(Shapes, TrackAddShape),
            ToolType.Screen => screen ??= new ScreenTool(Shapes, TrackAddShape),
            ToolType.Eraser => eraser ??= new EraserTool(Shapes),
            ToolType.ShotArc => shot ??= new ShotArcTool(Shapes, TrackAddShape),
            ToolType.Pen => pen ??= new PenTool(Shapes, TrackAddShape),
            _ => null
        };
    }
}
