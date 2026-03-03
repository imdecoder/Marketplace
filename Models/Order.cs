using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Marketplace.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("platformId")]
    public string PlatformId { get; set; } = null!;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();
}
