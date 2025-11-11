using System.Numerics;

namespace FastBoard.Core.Tools;

public enum ToolType { None, Select, Pen, Arrow, Dribble, Curve, Screen, ShotArc, Eraser }

public interface ITool
{
    ToolType Type { get; }
}

public interface IDrawingTool : ITool
{
    void Begin(Vector2 pt, float pressure = 0.5f);
    void Move(Vector2 pt, float pressure = 0.5f);
    void End(Vector2 pt, float pressure = 0.5f);
}
