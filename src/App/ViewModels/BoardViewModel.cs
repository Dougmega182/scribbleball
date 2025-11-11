using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.ViewModels;

public partial class BoardViewModel : ObservableObject
{
    public List<Shape> Shapes { get; } = new();

    public List<FastBoard.Core.Playback.FrameState> Frames { get; } = new();
    [ObservableProperty]
    private int currentFrameIndex = -1;

    public void CaptureFrame()
    {
        Frames.Add(FastBoard.Core.Playback.FrameState.Capture(Shapes));
        CurrentFrameIndex = Frames.Count - 1;
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
            var s = FastBoard.Core.Playback.Tween.LerpShape(a.Shapes[i], b.Shapes[i], t);
            lerped.Add(s);
        }
        return lerped;
    }
    public ToolType ActiveTool { get => activeTool; set => SetProperty(ref activeTool, value); }
    private ToolType activeTool = ToolType.None;

    private SelectTool? select;
    private ArrowTool? arrow;
    private DribbleTool? dribble;
    private CurveTool? curve;
    private ScreenTool? screen;
    private EraserTool? eraser;
    private ShotArcTool? shot;

    public IDrawingTool? GetActiveDrawingTool()
    {
        return ActiveTool switch
        {
            ToolType.Select => select ??= new SelectTool(Shapes),
            ToolType.Arrow => arrow ??= new ArrowTool(Shapes),
            ToolType.Dribble => dribble ??= new DribbleTool(Shapes),
            ToolType.Curve => curve ??= new CurveTool(Shapes),
            ToolType.Screen => screen ??= new ScreenTool(Shapes),
            ToolType.Eraser => eraser ??= new EraserTool(Shapes),
            ToolType.ShotArc => shot ??= new ShotArcTool(Shapes),
            _ => null
        };
    }
}
