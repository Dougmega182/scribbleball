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

    private ArrowTool? arrow;
    private DribbleTool? dribble;

    public IDrawingTool? GetActiveDrawingTool()
    {
        return ActiveTool switch
        {
            ToolType.Arrow => arrow ??= new ArrowTool(Shapes),
            ToolType.Dribble => dribble ??= new DribbleTool(Shapes),
            _ => null
        };
    }
}
