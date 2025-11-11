using FastBoard.Core.Models;

namespace FastBoard.Core.Playback;

public class FrameState
{
    public List<Shape> Shapes { get; set; } = new();

    public static FrameState Capture(IEnumerable<Shape> shapes)
    {
        var fs = new FrameState();
        foreach (var s in shapes)
        {
            fs.Shapes.Add(CloneShape(s));
        }
        return fs;
    }

    public static Shape CloneShape(Shape s)
    {
        switch (s)
        {
            case Token t:
                return new Token{ Name=t.Name, ImagePath=t.ImagePath, Position=t.Position, Rotation=t.Rotation, Scale=t.Scale };
            case ArrowShape a:
                return new ArrowShape{ Start=a.Start, End=a.End, Thickness=a.Thickness, HeadLength=a.HeadLength, HeadAngleDeg=a.HeadAngleDeg };
            case DashedShape d:
                return new DashedShape{ Points = d.Points.Select(x=>x).ToList(), Thickness=d.Thickness, Dash=d.Dash, Gap=d.Gap };
            case FastBoard.Core.Tools.CurveShape c:
                return new FastBoard.Core.Tools.CurveShape{ P0=c.P0, P1=c.P1, P2=c.P2, Thickness=c.Thickness };
            case FastBoard.Core.Tools.ScreenShape r:
                return new FastBoard.Core.Tools.ScreenShape{ Center=r.Center, Size=r.Size, CornerRadius=r.CornerRadius, Rotation=r.Rotation, Thickness=r.Thickness };
            default:
                throw new NotSupportedException($"Clone not supported: {s.GetType().Name}");
        }
    }
}
