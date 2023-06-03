using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BespokeBot
{
    public class DbHelper
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;

        public DbHelper(string connectionString, string dbName)
        {
            _mongoClient = new MongoClient(connectionString);
            _database = _mongoClient.GetDatabase(dbName);
        }

        public async Task AddMemberAsync(DiscordMember discordMember)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            var result = collection.Find(filter).ToList();

            if (result.Count == 0)
            {
                var document = new BsonDocument
                {
                    { "discordId", discordMember.Id.ToString() },
                    { "bespokePoints", 0 },
                    { "warnings", 0 }
                };

                await collection.InsertOneAsync(document);
            }
        }

        public async Task DeleteMemberAsync(DiscordMember discordMember)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            await collection.DeleteOneAsync(filter);
        }

        public async Task AddBespokePointsAsync(DiscordMember discordMember, int points)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            var records = collection.Find(filter);

            if (records.ToList().Count > 0)
            {
                var document = collection.Find(filter).First();

                var pointsValue = document.GetValue("bespokePoints");

                if (pointsValue != null && pointsValue.IsInt32)
                {
                    int currentPoints = pointsValue.AsInt32;
                    int newPoints = currentPoints + points;

                    var update = Builders<BsonDocument>.Update.Set("bespokePoints", newPoints);

                    await collection.UpdateOneAsync(filter, update);
                }
            }
        }

        public int GetBespokePoints(DiscordMember discordMember)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            var records = collection.Find(filter);

            if (records.ToList().Count > 0)
            {
                var document = records.First();

                var warningsValue = document.GetValue("bespokePoints");

                if (warningsValue != null && warningsValue.IsInt32)
                {
                    return warningsValue.AsInt32;
                }
            }

            return 0;
        }

        public async Task AddWarningAsync(DiscordMember discordMember)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            var records = collection.Find(filter);

            if (records.ToList().Count > 0)
            {
                var document = records.First();

                var pointsValue = document.GetValue("warnings");

                if (pointsValue != null && pointsValue.IsInt32)
                {
                    int currentWarnings = pointsValue.AsInt32;

                    var update = Builders<BsonDocument>.Update.Set("bespokePoints", currentWarnings + 1);

                    await collection.UpdateOneAsync(filter, update);
                }
            }
        }

        public int GetWarnings(DiscordMember discordMember)
        {
            var collection = _database.GetCollection<BsonDocument>("members");

            var filter = Builders<BsonDocument>.Filter.Eq("discordId", discordMember.Id.ToString());

            var records = collection.Find(filter);

            if (records.ToList().Count > 0)
            {
                var document = records.First();

                var warningsValue = document.GetValue("warnings");

                if (warningsValue != null && warningsValue.IsInt32)
                {
                    return warningsValue.AsInt32;
                }
            }

            return 0;
        }

        public async Task BlacklistWordAsync(string word)
        {
            var collection = _database.GetCollection<BsonDocument>("blacklisted_words");
            var document = new BsonDocument
            {
                { "word", word }
            };

            await collection.InsertOneAsync(document);
        }

        public async Task WhitelistWordAsync(string word)
        {
            var collection = _database.GetCollection<BsonDocument>("blacklisted_words");

            var filter = Builders<BsonDocument>.Filter.Eq("word", word);

            await collection.DeleteOneAsync(filter);
        }

        public bool IsBlacklisted(string word)
        {
            var collection = _database.GetCollection<BsonDocument>("blacklisted_words");

            var filter = Builders<BsonDocument>.Filter.Eq("word", word);
            var records = collection.Find(filter);

            if (records.ToList().Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool ContainsBlacklistedWords(string text)
        {
            var collection = _database.GetCollection<BsonDocument>("blacklisted_words");
            var words = text.Split(' ');

            foreach (var word in words)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("word", word);
                var records = collection.Find(filter);

                if (records.ToList().Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
