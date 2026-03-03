using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Marketplace.Models;

namespace Marketplace.Services;

public class OrderAnalysisService
{
    private readonly IMongoCollection<Order> _ordersCollection;
    private readonly IMongoCollection<Platform> _platformsCollection;

    public OrderAnalysisService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _ordersCollection = mongoDatabase.GetCollection<Order>(databaseSettings.Value.OrdersCollectionName);
        _platformsCollection = mongoDatabase.GetCollection<Platform>(databaseSettings.Value.PlatformsCollectionName);
    }

    public async Task<List<Order>> GetAllOrdersAsync() =>
        await _ordersCollection.Find(_ => true).ToListAsync();

    public async Task<Order?> GetOrderByIdAsync(string id) =>
        await _ordersCollection.Find(o => o.Id == id).FirstOrDefaultAsync();

    public async Task CreateOrderAsync(Order newOrder) =>
        await _ordersCollection.InsertOneAsync(newOrder);

    public async Task UpdateOrderAsync(string id, Order updatedOrder) =>
        await _ordersCollection.ReplaceOneAsync(o => o.Id == id, updatedOrder);

    public async Task DeleteOrderAsync(string id) =>
        await _ordersCollection.DeleteOneAsync(o => o.Id == id);

    private static decimal CalculateNetProfit(OrderItem item)
    {
        return (item.SalePrice - item.PurchasePrice - (item.SalePrice * item.CommissionRate / 100) - item.ShippingCost) * item.Quantity;
    }

    public async Task<object> GetSummaryAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var totalOrders = orders.Count;
        var totalProductCount = orders.SelectMany(o => o.Items).Sum(i => i.Quantity);
        var totalTurnover = orders.SelectMany(o => o.Items).Sum(i => i.SalePrice * i.Quantity);
        var totalNetProfit = orders.SelectMany(o => o.Items).Sum(i => CalculateNetProfit(i));

        return new { totalOrders, totalProductCount, totalTurnover, totalNetProfit };
    }

    public async Task<object> GetPlatformReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var platforms = await _platformsCollection.Find(_ => true).ToListAsync();
        var platformDict = platforms.ToDictionary(p => p.Id!, p => p.Name);

        var platformStats = orders.GroupBy(o => o.PlatformId)
            .Select(g =>
            {
                var turnover = g.SelectMany(o => o.Items).Sum(i => i.SalePrice * i.Quantity);
                var netProfit = g.SelectMany(o => o.Items).Sum(i => CalculateNetProfit(i));
                var profitMargin = turnover > 0 ? (netProfit / turnover) * 100 : 0;
                var platformName = platformDict.TryGetValue(g.Key, out var name) ? name : "Unknown";
                return new
                {
                    platform = platformName,
                    platformId = g.Key,
                    turnover,
                    netProfit,
                    profitMargin = Math.Round(profitMargin, 2)
                };
            }).ToList();

        return platformStats;
    }

    public async Task<object> GetLossReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var platforms = await _platformsCollection.Find(_ => true).ToListAsync();
        var platformDict = platforms.ToDictionary(p => p.Id!, p => p.Name);

        var lossProducts = orders.SelectMany(o => o.Items.Select(i => new
        {
            orderId = o.Id,
            platform = platformDict.TryGetValue(o.PlatformId, out var name) ? name : "Unknown",
            platformId = o.PlatformId,
            date = o.Date,
            item = i,
            netProfit = CalculateNetProfit(i)
        }))
        .Where(x => x.netProfit < 0)
        .ToList();

        return lossProducts;
    }

    public async Task<object> GetAnomalyReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var allItems = orders.SelectMany(o => o.Items).ToList();
        var platforms = await _platformsCollection.Find(_ => true).ToListAsync();
        var platformDict = platforms.ToDictionary(p => p.Id!, p => p.Name);

        var averagePrices = allItems.GroupBy(i => i.Name)
            .ToDictionary(g => g.Key, g => g.Average(i => i.SalePrice));

        var anomalies = orders.SelectMany(o => o.Items.Select(i => new
        {
            orderId = o.Id,
            platform = platformDict.TryGetValue(o.PlatformId, out var name) ? name : "Unknown",
            platformId = o.PlatformId,
            date = o.Date,
            item = i,
            averagePrice = averagePrices[i.Name],
            deviationPercentage = Math.Abs((i.SalePrice - averagePrices[i.Name]) / averagePrices[i.Name] * 100)
        }))
        .Where(x => x.deviationPercentage > 50)
        .ToList();

        return anomalies;
    }

    public async Task<object> GetTrendReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();

        var dailyStats = orders.GroupBy(o => o.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                dailyTurnover = g.SelectMany(o => o.Items).Sum(i => i.SalePrice * i.Quantity),
                dailyNetProfit = g.SelectMany(o => o.Items).Sum(i => CalculateNetProfit(i))
            }).ToList();

        return dailyStats;
    }

    public async Task<object> GetRiskReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var platforms = await _platformsCollection.Find(_ => true).ToListAsync();
        var platformDict = platforms.ToDictionary(p => p.Id!, p => p.Name);

        var riskLevels = orders.SelectMany(o => o.Items.Select(i =>
        {
            var turnover = i.SalePrice * i.Quantity;
            var netProfit = CalculateNetProfit(i);
            var profitMargin = turnover > 0 ? (netProfit / turnover) * 100 : 0;

            string riskLevel = "Low";
            if (profitMargin < 5 || netProfit < 0) riskLevel = "High";
            else if (profitMargin >= 5 && profitMargin <= 15) riskLevel = "Medium";

            return new
            {
                orderId = o.Id,
                platform = platformDict.TryGetValue(o.PlatformId, out var name) ? name : "Unknown",
                platformId = o.PlatformId,
                item = i,
                profitMargin = Math.Round(profitMargin, 2),
                riskLevel
            };
        }))
        .ToList();

        return riskLevels;
    }
}
