namespace FastBoard.Core.Models;

public class Playbook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<Play> Plays { get; set; } = new();
}

public class Play
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<Frame> Frames { get; set; } = new();
}

public class Frame
{
    public int Id { get; set; }
    public double Time { get; set; }
}
