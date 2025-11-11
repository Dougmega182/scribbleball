using System.Numerics;

namespace FastBoard.Core.Models;

public abstract class Shape
{
    public bool Selected { get; set; }
}

public class ArrowShape : Shape
{
    public Vector2 Start { get; set; }
    public Vector2 End { get; set; }
    public float Thickness { get; set; } = 3f;
    public float HeadLength { get; set; } = 12f;
    public float HeadAngleDeg { get; set; } = 28f;
}

public class DashedShape : Shape
{
    public List<Vector2> Points { get; set; } = new();
    public float Thickness { get; set; } = 3f;
    public float Dash { get; set; } = 6f;
    public float Gap { get; set; } = 6f;
}
