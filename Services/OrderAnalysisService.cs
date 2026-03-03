using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Marketplace.Models;

namespace Marketplace.Services;

public class OrderAnalysisService
{
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrderAnalysisService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _ordersCollection = mongoDatabase.GetCollection<Order>(databaseSettings.Value.OrdersCollectionName);
    }

    public async Task CreateOrderAsync(Order newOrder)
    {
        await _ordersCollection.InsertOneAsync(newOrder);
    }

    private decimal CalculateNetProfit(OrderItem item)
    {
        return (item.SalePrice - item.PurchasePrice - (item.SalePrice * item.CommissionRate / 100) - item.ShippingCost) * item.Quantity;
    }

    public async Task<object> GetSummaryAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();
        var totalOrders = orders.Count;
        var totalProductCount = orders.Sum(o => o.Items.Sum(i => i.Quantity));
        var totalTurnover = orders.Sum(o => o.Items.Sum(i => i.SalePrice * i.Quantity));
        var totalNetProfit = orders.Sum(o => o.Items.Sum(i => CalculateNetProfit(i)));

        return new { totalOrders, totalProductCount, totalTurnover, totalNetProfit };
    }

    public async Task<object> GetPlatformReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();

        var platformStats = orders.GroupBy(o => o.Platform)
            .Select(g =>
            {
                var turnover = g.Sum(o => o.Items.Sum(i => i.SalePrice * i.Quantity));
                var netProfit = g.Sum(o => o.Items.Sum(i => CalculateNetProfit(i)));
                var profitMargin = turnover > 0 ? (netProfit / turnover) * 100 : 0;
                return new
                {
                    platform = g.Key,
                    turnover = turnover,
                    netProfit = netProfit,
                    profitMargin = Math.Round(profitMargin, 2)
                };
            }).ToList();

        return platformStats;
    }

    public async Task<object> GetLossReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();

        var lossProducts = orders.SelectMany(o => o.Items.Select(i => new
        {
            orderId = o.Id,
            platform = o.Platform,
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

        var averagePrices = allItems.GroupBy(i => i.Name)
            .ToDictionary(g => g.Key, g => g.Average(i => i.SalePrice));

        var anomalies = orders.SelectMany(o => o.Items.Select(i => new
        {
            orderId = o.Id,
            platform = o.Platform,
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
                dailyTurnover = g.Sum(o => o.Items.Sum(i => i.SalePrice * i.Quantity)),
                dailyNetProfit = g.Sum(o => o.Items.Sum(i => CalculateNetProfit(i)))
            }).ToList();

        return dailyStats;
    }

    public async Task<object> GetRiskReportAsync()
    {
        var orders = await _ordersCollection.Find(_ => true).ToListAsync();

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
                platform = o.Platform,
                item = i,
                profitMargin = Math.Round(profitMargin, 2),
                riskLevel = riskLevel
            };
        }))
        .ToList();

        return riskLevels;
    }
}
