using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LivestreamService.Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBSettings _settings;

        public MongoDbContext(IOptions<MongoDBSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<LivestreamChat> LivestreamChats =>
            _database.GetCollection<LivestreamChat>(_settings.LivestreamChatCollectionName);

        public IMongoCollection<ChatRoom> ChatRooms =>
            _database.GetCollection<ChatRoom>(_settings.ChatRoomCollectionName);

        public IMongoCollection<ChatMessage> ChatMessages =>
            _database.GetCollection<ChatMessage>(_settings.ChatMessageCollectionName);

        public IMongoDatabase Database => _database;
    }
}