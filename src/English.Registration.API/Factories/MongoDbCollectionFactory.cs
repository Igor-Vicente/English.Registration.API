﻿using MongoDB.Driver;

namespace English.Registration.API.Factories
{
    public class MongoDbCollectionFactory
    {
        public const string APPLICATION_USERS_COLLECTION = "appusers";
        public const string MODULES_COLLECTION = "modules";

        private readonly string _connectionString;

        public MongoDbCollectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MongoDb")
                ?? throw new InvalidOperationException("ConnectionString not defined in 'app settings'");
        }

        public IMongoCollection<T> GetCollection<T>(string tableName)
        {
            var url = new MongoUrl(_connectionString);
            var db = new MongoClient(MongoClientSettings.FromUrl(url)).GetDatabase(url.DatabaseName
                ?? throw new InvalidOperationException("Database name not defined in 'app settings'"));
            return db.GetCollection<T>(tableName);
        }
    }
}
