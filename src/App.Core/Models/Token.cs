using System.Numerics;

namespace FastBoard.Core.Models;

public class Token : Shape
{
    public string Name { get; set; } = "Player";
    public int Number { get; set; } = 0;
    public string? ImagePath { get; set; }
    public byte[]? ImageBytes { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public float Scale { get; set; } = 1f;
    public uint Color { get; set; } = 0xFF1E90FF; // DodgerBlue ARGB
}
