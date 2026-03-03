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

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = null!;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();
}
