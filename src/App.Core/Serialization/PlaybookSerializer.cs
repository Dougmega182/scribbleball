using System.Text.Json;
using System.Text.Json.Serialization;
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

    public static string ToJson(Playbook playbook) => JsonSerializer.Serialize(playbook, Options);
    public static Playbook FromJson(string json) => JsonSerializer.Deserialize<Playbook>(json, Options) ?? new Playbook();
}
