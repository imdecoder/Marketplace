using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Marketplace.Models;

public class OrderItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("purchasePrice")]
    public decimal PurchasePrice { get; set; }

    [JsonPropertyName("salePrice")]
    public decimal SalePrice { get; set; }

    [JsonPropertyName("commissionRate")]
    public decimal CommissionRate { get; set; }

    [JsonPropertyName("shippingCost")]
    public decimal ShippingCost { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
