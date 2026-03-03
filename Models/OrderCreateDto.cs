using System.Text.Json.Serialization;

namespace Marketplace.Models;

public class OrderCreateDto
{
    [JsonPropertyName("platformId")]
    public string PlatformId { get; set; } = null!;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();
}
