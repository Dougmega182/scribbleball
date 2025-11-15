using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using FastBoard.Core.Models;

namespace FastBoard.Core.Serialization;

public static class PlaybookSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToJson(Playbook playbook)
    {
        // Custom serialize frames' shapes using BoardStateSerializer to include embedded images
        var root = new JsonObject
        {
            ["id"] = playbook.Id,
            ["name"] = playbook.Name,
            ["plays"] = new JsonArray()
        };
        var playsArr = (JsonArray)root["plays"]!;
        foreach (var p in playbook.Plays)
        {
            var po = new JsonObject
            {
                ["id"] = p.Id,
                ["title"] = p.Title,
                ["frames"] = new JsonArray()
            };
            var framesArr = (JsonArray)po["frames"]!;
            foreach (var f in p.Frames)
            {
                var fo = new JsonObject
                {
                    ["id"] = f.Id,
                    ["duration"] = f.Duration,
                    ["shapes"] = JsonNode.Parse(FastBoard.Core.Serialization.BoardStateSerializer.ToJson(f.Shapes))
                };
                framesArr.Add(fo);
            }
            playsArr.Add(po);
        }
        return root.ToJsonString(Options);
    }

    public static Playbook FromJson(string json)
    {
        var root = JsonNode.Parse(json)?.AsObject();
        if (root == null) return new Playbook();
        var pb = new Playbook
        {
            Id = (int)(root["id"]?.GetValue<long>() ?? 0),
            Name = (string?)root["name"] ?? string.Empty,
            Plays = new List<Play>()
        };
        var playsArr = root["plays"]?.AsArray();
        if (playsArr != null)
        {
            foreach (var pn in playsArr)
            {
                var po = pn!.AsObject();
                var play = new Play
                {
                    Id = (int)(po["id"]?.GetValue<long>() ?? 0),
                    Title = (string?)po["title"] ?? string.Empty,
                    Frames = new List<FastBoard.Core.Models.Frame>()
                };
                var framesArr = po["frames"]?.AsArray();
                if (framesArr != null)
                {
                    foreach (var fn in framesArr)
                    {
                        var fo = fn!.AsObject();
                        var frame = new FastBoard.Core.Models.Frame
                        {
                            Id = (int)(fo["id"]?.GetValue<long>() ?? 0),
                            Duration = (double)(fo["duration"]?.GetValue<double>() ?? 1.0),
                            Shapes = FastBoard.Core.Serialization.BoardStateSerializer.FromJson(fo["shapes"]!.ToJsonString())
                        };
                        play.Frames.Add(frame);
                    }
                }
                pb.Plays.Add(play);
            }
        }
        return pb;
    }
}
