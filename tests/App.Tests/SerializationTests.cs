using FastBoard.Core.Models;
using FastBoard.Core.Serialization;
using Xunit;

namespace App.Tests;

public class SerializationTests
{
    [Fact]
    public void Playbook_Roundtrip()
    {
        var pb = new Playbook { Name = "Test" };
        pb.Plays.Add(new Play { Title = "P1", Frames = { new Frame{ Time=0 }, new Frame{ Time=1 } } });
        var json = PlaybookSerializer.ToJson(pb);
        var pb2 = PlaybookSerializer.FromJson(json);
        Assert.Equal("Test", pb2.Name);
        Assert.Single(pb2.Plays);
        Assert.Equal(2, pb2.Plays[0].Frames.Count);
    }
}
