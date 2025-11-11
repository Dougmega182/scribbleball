namespace FastBoard.Core.Models;

public class Playbook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Play> Plays { get; set; } = new();
}

public class Play
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Frame> Frames { get; set; } = new();
}

public class Frame
{
    public int Id { get; set; }
    public double Time { get; set; }
}
