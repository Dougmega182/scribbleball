using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
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
                        Position = new Vector2(GetF(o["x"]), GetF(o["y"])),
                        Rotation = GetF(o["rot"],0f),
                        Scale = GetF(o["scale"],1f)
                    });
                    break;
                case "arrow":
                    list.Add(new ArrowShape{
                        Start = new Vector2(GetF(o["x1"]), GetF(o["y1"])),
                        End = new Vector2(GetF(o["x2"]), GetF(o["y2"])),
                        Thickness = GetF(o["th"],3f)
                    });
                    break;
                case "dribble":
                    var ptsArr = o["pts"]?.AsArray();
                    var dpts = new List<Vector2>();
                    if (ptsArr != null)
                    {
                        var tmp = ptsArr.Select(n => (float)n!.GetValue<double>()).ToArray();
                        for (int i=0; i+1<tmp.Length; i+=2) dpts.Add(new Vector2(tmp[i], tmp[i+1]));
                    }
                    list.Add(new DashedShape{ Points=dpts, Thickness=GetF(o["th"],3f), Dash=GetF(o["dash"],6f), Gap=GetF(o["gap"],6f) });
                    break;
                case "curve":
                    list.Add(new CurveShape{ P0 = ParseVec(o["p0"]!.AsArray()), P1 = ParseVec(o["p1"]!.AsArray()), P2 = ParseVec(o["p2"]!.AsArray()), Thickness=GetF(o["th"],3f) });
                    break;
                case "screen":
                    list.Add(new ScreenShape{ Center=ParseVec(o["c"]!.AsArray()), Size=ParseVec(o["sz"]!.AsArray()), CornerRadius=GetF(o["cr"],0f), Rotation=GetF(o["rot"],0f), Thickness=GetF(o["th"],3f) });
                    break;
                case "shotarc":
                    list.Add(new ShotArcShape{ Center=ParseVec(o["c"]!.AsArray()), Radius=GetF(o["r"],20f), StartAngleDeg=GetF(o["a"],0f), SweepDeg=GetF(o["sweep"],90f), Thickness=GetF(o["th"],3f) });
                    break;
            }
        }
        return list;
    }

    private static JsonArray BuildPointsArray(List<Vector2> pts)
    {
        var arr = new JsonArray();
        foreach (var p in pts) { arr.Add((double)p.X); arr.Add((double)p.Y); }
        return arr;
    }

    private static float GetF(JsonNode? n, float def = 0f)
    {
        return n is null ? def : (float)n.GetValue<double>();
    }

    private static Vector2 ParseVec(JsonArray a)
    {
        return new Vector2((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>());
    }
}
