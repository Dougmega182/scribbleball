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
                    ["number"] = t.Number,
                    ["color"] = (long)t.Color,
                    ["imagePath"] = t.ImagePath,
                    ["imageBase64"] = (t.ImageBytes != null && t.ImageBytes.Length > 0) ? Convert.ToBase64String(t.ImageBytes) : (File.Exists(t.ImagePath ?? "") ? Convert.ToBase64String(File.ReadAllBytes(t.ImagePath!)) : null),
                    ["x"] = t.Position.X,
                    ["y"] = t.Position.Y,
                    ["rot"] = t.Rotation,
                    ["scale"] = t.Scale,
                    ["ease"] = t.Easing.ToString().ToLower()
                },
                ArrowShape a => new JsonObject{
                    ["type"] = "arrow",
                    ["x1"] = a.Start.X, ["y1"] = a.Start.Y,
                    ["x2"] = a.End.X, ["y2"] = a.End.Y,
                    ["th"] = a.Thickness,
                    ["ease"] = a.Easing.ToString().ToLower()
                },
                DashedShape d => new JsonObject{
                    ["type"] = "dribble",
                    ["pts"] = BuildPointsArray(d.Points),
                    ["th"] = d.Thickness, ["dash"] = d.Dash, ["gap"] = d.Gap,
                    ["ease"] = d.Easing.ToString().ToLower()
                },
                CurveShape c => new JsonObject{
                    ["type"] = "curve",
                    ["p0"] = new JsonArray(c.P0.X, c.P0.Y),
                    ["p1"] = new JsonArray(c.P1.X, c.P1.Y),
                    ["p2"] = new JsonArray(c.P2.X, c.P2.Y),
                    ["th"] = c.Thickness,
                    ["ease"] = c.Easing.ToString().ToLower()
                },
                ScreenShape r => new JsonObject{
                    ["type"] = "screen",
                    ["c"] = new JsonArray(r.Center.X, r.Center.Y),
                    ["sz"] = new JsonArray(r.Size.X, r.Size.Y),
                    ["cr"] = r.CornerRadius,
                    ["rot"] = r.Rotation,
                    ["th"] = r.Thickness,
                    ["ease"] = r.Easing.ToString().ToLower()
                },
                ShotArcShape sa => new JsonObject{
                    ["type"] = "shotarc",
                    ["c"] = new JsonArray(sa.Center.X, sa.Center.Y),
                    ["r"] = sa.Radius,
                    ["a"] = sa.StartAngleDeg,
                    ["sweep"] = sa.SweepDeg,
                    ["th"] = sa.Thickness,
                    ["ease"] = sa.Easing.ToString().ToLower()
                },
                StrokeShape st => new JsonObject{
                    ["type"] = "stroke",
                    ["pts"] = BuildPointsArray(st.Points),
                    ["prs"] = BuildFloatArray(st.Pressures),
                    ["tb"] = st.ThicknessBase,
                    ["ease"] = st.Easing.ToString().ToLower()
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
                        Number = (int)(o["number"]?.GetValue<long>() ?? 0),
                        Color = (uint)(o["color"]?.GetValue<long>() ?? 0xFF1E90FF),
                        ImagePath = (string?)o["imagePath"],
                        ImageBytes = ParseBase64((string?)o["imageBase64"]),
                        Position = new Vector2(GetF(o["x"]), GetF(o["y"])),
                        Rotation = GetF(o["rot"],0f),
                        Scale = GetF(o["scale"],1f),
                        Easing = ParseEase(o["ease"]) 
                    });
                    break;
                case "arrow":
                    list.Add(new ArrowShape{
                        Start = new Vector2(GetF(o["x1"]), GetF(o["y1"])),
                        End = new Vector2(GetF(o["x2"]), GetF(o["y2"])),
                        Thickness = GetF(o["th"],3f),
                        Easing = ParseEase(o["ease"])
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
                    list.Add(new DashedShape{ Points=dpts, Thickness=GetF(o["th"],3f), Dash=GetF(o["dash"],6f), Gap=GetF(o["gap"],6f), Easing = ParseEase(o["ease"]) });
                    break;
                case "curve":
                    list.Add(new CurveShape{ P0 = ParseVec(o["p0"]!.AsArray()), P1 = ParseVec(o["p1"]!.AsArray()), P2 = ParseVec(o["p2"]!.AsArray()), Thickness=GetF(o["th"],3f), Easing = ParseEase(o["ease"]) });
                    break;
                case "screen":
                    list.Add(new ScreenShape{ Center=ParseVec(o["c"]!.AsArray()), Size=ParseVec(o["sz"]!.AsArray()), CornerRadius=GetF(o["cr"],0f), Rotation=GetF(o["rot"],0f), Thickness=GetF(o["th"],3f), Easing = ParseEase(o["ease"]) });
                    break;
                case "shotarc":
                    list.Add(new ShotArcShape{ Center=ParseVec(o["c"]!.AsArray()), Radius=GetF(o["r"],20f), StartAngleDeg=GetF(o["a"],0f), SweepDeg=GetF(o["sweep"],90f), Thickness=GetF(o["th"],3f), Easing = ParseEase(o["ease"]) });
                    break;
                case "stroke":
                    var pArr = o["pts"]?.AsArray();
                    var prArr = o["prs"]?.AsArray();
                    var pts = new List<Vector2>();
                    var prs = new List<float>();
                    if (pArr != null)
                    {
                        var tmp = pArr.Select(n => (float)n!.GetValue<double>()).ToArray();
                        for (int i=0; i+1<tmp.Length; i+=2) pts.Add(new Vector2(tmp[i], tmp[i+1]));
                    }
                    if (prArr != null)
                    {
                        foreach (var n in prArr) prs.Add((float)n!.GetValue<double>());
                    }
                    list.Add(new StrokeShape{ Points = pts, Pressures = prs, ThicknessBase = GetF(o["tb"], 2f), Easing = ParseEase(o["ease"]) });
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

    private static JsonArray BuildFloatArray(List<float> vals)
    {
        var arr = new JsonArray();
        foreach (var v in vals) arr.Add((double)v);
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

    private static EaseType ParseEase(JsonNode? n)
    {
        var s = (n as JsonValue)?.GetValue<string?>();
        return s?.ToLower() switch
        {
            "easeinout" => EaseType.EaseInOut,
            _ => EaseType.Linear
        };
    }

    private static byte[]? ParseBase64(string? b64)
    {
        if (string.IsNullOrEmpty(b64)) return null;
        try { return Convert.FromBase64String(b64); } catch { return null; }
    }
}
