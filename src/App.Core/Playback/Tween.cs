using System.Numerics;
using FastBoard.Core.Models;

namespace FastBoard.Core.Playback;

public static class Tween
{
    public static float EaseInOut(float t)
    {
        return t < 0.5f ? 2*t*t : -1 + (4 - 2*t)*t;
    }

    public static float ApplyEase(float t, FastBoard.Core.Models.EaseType ease)
    {
        return ease switch
        {
            FastBoard.Core.Models.EaseType.EaseInOut => EaseInOut(Math.Clamp(t,0f,1f)),
            _ => Math.Clamp(t,0f,1f)
        };
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b-a)*t;

    public static Shape LerpShape(Shape a, Shape b, float t)
    {
        switch (a, b)
        {
            case (Token ta, Token tb):
                return new Token{ Name=tb.Name, ImagePath=tb.ImagePath, Position=Lerp(ta.Position,tb.Position,t), Rotation=ta.Rotation + (tb.Rotation-ta.Rotation)*t, Scale=ta.Scale + (tb.Scale-ta.Scale)*t };
            case (ArrowShape aa, ArrowShape ab):
                return new ArrowShape{ Start=Lerp(aa.Start,ab.Start,t), End=Lerp(aa.End,ab.End,t), Thickness=ab.Thickness, HeadLength=ab.HeadLength, HeadAngleDeg=ab.HeadAngleDeg };
            case (DashedShape da, DashedShape db):
                var count = Math.Min(da.Points.Count, db.Points.Count);
                var pts = new List<Vector2>();
                for (int i=0;i<count;i++) pts.Add(Lerp(da.Points[i], db.Points[i], t));
                return new DashedShape{ Points=pts, Thickness=db.Thickness, Dash=db.Dash, Gap=db.Gap };
            case (FastBoard.Core.Tools.CurveShape ca, FastBoard.Core.Tools.CurveShape cb):
                return new FastBoard.Core.Tools.CurveShape{ P0=Lerp(ca.P0,cb.P0,t), P1=Lerp(ca.P1,cb.P1,t), P2=Lerp(ca.P2,cb.P2,t), Thickness=cb.Thickness };
            case (FastBoard.Core.Tools.ScreenShape ra, FastBoard.Core.Tools.ScreenShape rb):
                return new FastBoard.Core.Tools.ScreenShape{ Center=Lerp(ra.Center,rb.Center,t), Size=Lerp(ra.Size,rb.Size,t), CornerRadius=rb.CornerRadius, Rotation=ra.Rotation + (rb.Rotation-ra.Rotation)*t, Thickness=rb.Thickness };
            default:
                return b; // fallback
        }
    }
}
