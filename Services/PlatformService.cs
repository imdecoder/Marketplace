using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Marketplace.Models;

namespace Marketplace.Services;

public class PlatformService
{
    private readonly IMongoCollection<Platform> _platformsCollection;

    public PlatformService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _platformsCollection = mongoDatabase.GetCollection<Platform>(databaseSettings.Value.PlatformsCollectionName);
    }

    public async Task<List<Platform>> GetAllAsync() =>
        await _platformsCollection.Find(_ => true).ToListAsync();

    public async Task<Platform?> GetByIdAsync(string id) =>
        await _platformsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Platform newPlatform) =>
        await _platformsCollection.InsertOneAsync(newPlatform);

    public async Task UpdateAsync(string id, Platform updatedPlatform) =>
        await _platformsCollection.ReplaceOneAsync(p => p.Id == id, updatedPlatform);

    public async Task DeleteAsync(string id) =>
        await _platformsCollection.DeleteOneAsync(p => p.Id == id);
}
