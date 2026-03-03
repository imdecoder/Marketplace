using System.Text.Json.Serialization;

namespace Marketplace.Models;

public class PlatformCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
