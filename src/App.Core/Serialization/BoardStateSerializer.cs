using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using FastBoard.Core.Models;
using FastBoard.Core.Tools;

namespace FastBoard.Core.Serialization;

public static class BoardStateSerializer
{
    public static string ToJson(IEnumerable<Shape> shapes)
    {
        var arr = new JsonArray();
        foreach (var s in shapes)
        {
            JsonObject o = s switch
            {
                Token t => new JsonObject{
                    ["type"] = "token",
                    ["name"] = t.Name,
                    ["imagePath"] = t.ImagePath,
                    ["x"] = t.Position.X,
                    ["y"] = t.Position.Y,
                    ["rot"] = t.Rotation,
                    ["scale"] = t.Scale
                },
                ArrowShape a => new JsonObject{
                    ["type"] = "arrow",
                    ["x1"] = a.Start.X, ["y1"] = a.Start.Y,
                    ["x2"] = a.End.X, ["y2"] = a.End.Y,
                    ["th"] = a.Thickness
                },
                DashedShape d => new JsonObject{
                    ["type"] = "dribble",
                    ["pts"] = BuildPointsArray(d.Points),
                    ["th"] = d.Thickness, ["dash"] = d.Dash, ["gap"] = d.Gap
                },
                CurveShape c => new JsonObject{
                    ["type"] = "curve",
                    ["p0"] = new JsonArray(c.P0.X, c.P0.Y),
                    ["p1"] = new JsonArray(c.P1.X, c.P1.Y),
                    ["p2"] = new JsonArray(c.P2.X, c.P2.Y),
                    ["th"] = c.Thickness
                },
                ScreenShape r => new JsonObject{
                    ["type"] = "screen",
                    ["c"] = new JsonArray(r.Center.X, r.Center.Y),
                    ["sz"] = new JsonArray(r.Size.X, r.Size.Y),
                    ["cr"] = r.CornerRadius,
                    ["rot"] = r.Rotation,
                    ["th"] = r.Thickness
                },
                ShotArcShape sa => new JsonObject{
                    ["type"] = "shotarc",
                    ["c"] = new JsonArray(sa.Center.X, sa.Center.Y),
                    ["r"] = sa.Radius,
                    ["a"] = sa.StartAngleDeg,
                    ["sweep"] = sa.SweepDeg,
                    ["th"] = sa.Thickness
                },
                _ => new JsonObject{ ["type"]="unknown" }
            };
            arr.Add(o);
        }
        return arr.ToJsonString(new JsonSerializerOptions{ WriteIndented = true });
    }

    public static List<Shape> FromJson(string json)
    {
        var list = new List<Shape>();
        var arr = JsonNode.Parse(json)?.AsArray();
        if (arr == null) return list;
        foreach (var node in arr)
        {
            var o = node!.AsObject();
            var type = (string?)o["type"];
            switch (type)
            {
                case "token":
                    list.Add(new Token{
                        Name = (string?)o["name"] ?? "",
                        ImagePath = (string?)o["imagePath"],
                        Position = new Vector2((float?)o["x"] ?? 0f, (float?)o["y"] ?? 0f),
                        Rotation = (float?)o["rot"] ?? 0f,
                        Scale = (float?)o["scale"] ?? 1f
                    });
                    break;
                case "arrow":
                    list.Add(new ArrowShape{
                        Start = new Vector2((float?)o["x1"] ?? 0f, (float?)o["y1"] ?? 0f),
                        End = new Vector2((float?)o["x2"] ?? 0f, (float?)o["y2"] ?? 0f),
                        Thickness = (float?)o["th"] ?? 3f
                    });
                    break;
                case "dribble":
                    var pts = o["pts"]?.AsArray()?.Select(n => (float)n!.GetValue<double>()).ToArray() ?? Array.Empty<float>();
                    var dpts = new List<Vector2>();
                    for (int i=0;i+1<pts.Length;i+=2) dpts.Add(new Vector2(pts[i], pts[i+1]));
                    list.Add(new DashedShape{ Points=dpts, Thickness=(float?)o["th"] ?? 3f, Dash=(float?)o["dash"] ?? 6f, Gap=(float?)o["gap"] ?? 6f });
                    break;
                case "curve":
                    Vector2 V(JsonArray a) => new((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>());
                    list.Add(new CurveShape{ P0 = V(o["p0"]!.AsArray()), P1 = V(o["p1"]!.AsArray()), P2 = V(o["p2"]!.AsArray()), Thickness=(float?)o["th"] ?? 3f });
                    break;
                case "screen":
                    Vector2 V2(JsonArray a) => new((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>());
                    list.Add(new ScreenShape{ Center=V2(o["c"]!.AsArray()), Size=V2(o["sz"]!.AsArray()), CornerRadius=(float?)o["cr"] ?? 0f, Rotation=(float?)o["rot"] ?? 0f, Thickness=(float?)o["th"] ?? 3f });
                    break;
                case "shotarc":
                    Vector2 V3(JsonArray a) => new((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>());
                    list.Add(new ShotArcShape{ Center=V3(o["c"]!.AsArray()), Radius=(float?)o["r"] ?? 20f, StartAngleDeg=(float?)o["a"] ?? 0f, SweepDeg=(float?)o["sweep"] ?? 90f, Thickness=(float?)o["th"] ?? 3f });
                    break;
            }
        }
        return list;
    }
}
