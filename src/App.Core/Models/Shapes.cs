using System.Numerics;

namespace FastBoard.Core.Models;

public enum EaseType { Linear, EaseInOut }

public abstract class Shape
{
    public bool Selected { get; set; }
    public EaseType Easing { get; set; } = EaseType.Linear;
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

public class StrokeShape : Shape
{
    public List<Vector2> Points { get; set; } = new();
    public List<float> Pressures { get; set; } = new();
    public float ThicknessBase { get; set; } = 2f;
}
