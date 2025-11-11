using System.Numerics;

namespace FastBoard.Core.Models;

public class Token : Shape
{
    public string Name { get; set; } = "Player";
    public string? ImagePath { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public float Scale { get; set; } = 1f;
}
