using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Marketplace.Models;

namespace Marketplace.Services;

public class DatabaseSeeder
{
    private readonly IMongoCollection<Order> _ordersCollection;
    private readonly IMongoCollection<Platform> _platformsCollection;

    public DatabaseSeeder(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _ordersCollection = mongoDatabase.GetCollection<Order>(databaseSettings.Value.OrdersCollectionName);
        _platformsCollection = mongoDatabase.GetCollection<Platform>(databaseSettings.Value.PlatformsCollectionName);
    }

    public async Task SeedAsync()
    {
        var existingOrders = await _ordersCollection.CountDocumentsAsync(_ => true);
        if (existingOrders > 0) return;

        var platforms = new List<Platform>
        {
            new() { Name = "Trendyol" },
            new() { Name = "Hepsiburada" },
            new() { Name = "Amazon TR" },
            new() { Name = "N11" },
            new() { Name = "Çiçeksepeti" }
        };

        await _platformsCollection.InsertManyAsync(platforms);

        var products = new[]
        {
            new { Name = "Kablosuz Kulaklık",     PurchaseMin = 120m, PurchaseMax = 250m,  SaleMin = 199m,  SaleMax = 449m,  Commission = 12m },
            new { Name = "Akıllı Saat",           PurchaseMin = 300m, PurchaseMax = 600m,  SaleMin = 499m,  SaleMax = 999m,  Commission = 10m },
            new { Name = "Telefon Kılıfı",        PurchaseMin = 10m,  PurchaseMax = 30m,   SaleMin = 29m,   SaleMax = 89m,   Commission = 18m },
            new { Name = "Laptop Çantası",        PurchaseMin = 80m,  PurchaseMax = 180m,  SaleMin = 149m,  SaleMax = 349m,  Commission = 14m },
            new { Name = "USB-C Kablo",           PurchaseMin = 8m,   PurchaseMax = 25m,   SaleMin = 19m,   SaleMax = 69m,   Commission = 20m },
            new { Name = "Powerbank 20000mAh",    PurchaseMin = 150m, PurchaseMax = 300m,  SaleMin = 249m,  SaleMax = 549m,  Commission = 11m },
            new { Name = "Bluetooth Hoparlör",    PurchaseMin = 100m, PurchaseMax = 250m,  SaleMin = 179m,  SaleMax = 449m,  Commission = 13m },
            new { Name = "Oyuncu Mouse Pad",      PurchaseMin = 20m,  PurchaseMax = 60m,   SaleMin = 49m,   SaleMax = 149m,  Commission = 16m },
            new { Name = "Webcam 1080p",          PurchaseMin = 120m, PurchaseMax = 280m,  SaleMin = 199m,  SaleMax = 499m,  Commission = 12m },
            new { Name = "Monitör Standı",        PurchaseMin = 90m,  PurchaseMax = 200m,  SaleMin = 149m,  SaleMax = 399m,  Commission = 15m },
            new { Name = "Mekanik Klavye",        PurchaseMin = 200m, PurchaseMax = 450m,  SaleMin = 349m,  SaleMax = 799m,  Commission = 10m },
            new { Name = "Ergonomik Fare",        PurchaseMin = 60m,  PurchaseMax = 180m,  SaleMin = 99m,   SaleMax = 349m,  Commission = 14m },
            new { Name = "Tablet Kalemi",         PurchaseMin = 40m,  PurchaseMax = 120m,  SaleMin = 79m,   SaleMax = 249m,  Commission = 17m },
            new { Name = "Ekran Koruyucu",        PurchaseMin = 5m,   PurchaseMax = 20m,   SaleMin = 14m,   SaleMax = 59m,   Commission = 22m },
            new { Name = "Hızlı Şarj Aleti",     PurchaseMin = 50m,  PurchaseMax = 130m,  SaleMin = 89m,   SaleMax = 249m,  Commission = 13m },
            new { Name = "Airpods Kılıfı",        PurchaseMin = 8m,   PurchaseMax = 30m,   SaleMin = 19m,   SaleMax = 79m,   Commission = 20m },
            new { Name = "HDMI Kablo",            PurchaseMin = 15m,  PurchaseMax = 50m,   SaleMin = 34m,   SaleMax = 99m,   Commission = 18m },
            new { Name = "Notebook Soğutucu",     PurchaseMin = 70m,  PurchaseMax = 160m,  SaleMin = 119m,  SaleMax = 299m,  Commission = 14m },
            new { Name = "Araç Telefon Tutucu",   PurchaseMin = 20m,  PurchaseMax = 60m,   SaleMin = 39m,   SaleMax = 129m,  Commission = 16m },
            new { Name = "LED Masa Lambası",      PurchaseMin = 80m,  PurchaseMax = 200m,  SaleMin = 139m,  SaleMax = 379m,  Commission = 12m },
        };

        var random = new Random(42);
        var orders = new List<Order>();

        for (int i = 0; i < 100; i++)
        {
            var platform = platforms[random.Next(platforms.Count)];
            var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));
            var itemCount = random.Next(1, 4);

            var items = new List<OrderItem>();
            for (int j = 0; j < itemCount; j++)
            {
                var product = products[random.Next(products.Length)];
                var purchasePrice = Math.Round(product.PurchaseMin + (product.PurchaseMax - product.PurchaseMin) * (decimal)random.NextDouble(), 2);
                var salePrice = Math.Round(product.SaleMin + (product.SaleMax - product.SaleMin) * (decimal)random.NextDouble(), 2);
                var quantity = random.Next(1, 6);
                var shippingCost = Math.Round(5m + 30m * (decimal)random.NextDouble(), 2);

                if (random.NextDouble() < 0.15)
                {
                    salePrice = Math.Round(purchasePrice * (0.6m + 0.35m * (decimal)random.NextDouble()), 2);
                }

                items.Add(new OrderItem
                {
                    Name = product.Name,
                    PurchasePrice = purchasePrice,
                    SalePrice = salePrice,
                    Quantity = quantity,
                    CommissionRate = product.Commission,
                    ShippingCost = shippingCost
                });
            }

            orders.Add(new Order
            {
                PlatformId = platform.Id!,
                Date = orderDate,
                Items = items
            });
        }

        await _ordersCollection.InsertManyAsync(orders);
    }
}
