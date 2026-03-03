using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Marketplace.Models;

public class Platform
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
