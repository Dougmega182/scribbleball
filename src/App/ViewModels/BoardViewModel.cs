using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.ViewModels;

public partial class BoardViewModel : ObservableObject
{
    public List<Shape> Shapes { get; } = new();
    public ToolType ActiveTool { get => activeTool; set => SetProperty(ref activeTool, value); }
    private ToolType activeTool = ToolType.None;

    private SelectTool? select;
    private ArrowTool? arrow;
    private DribbleTool? dribble;
    private CurveTool? curve;
    private ScreenTool? screen;
    private EraserTool? eraser;

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
            _ => null
        };
    }
}
