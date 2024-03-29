﻿using System.Configuration;
using FeelKnitService.Model;
using FeelKnitService.Properties;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace FeelKnitService
{
    
    public class FeelingsContext
    {

        public MongoDatabase Database;

        public FeelingsContext()
        {
            var conString = ConfigurationManager.AppSettings.Get("MONGOHQ_URL") ??
                ConfigurationManager.AppSettings.Get("MONGOLAB_URI") ??
                "mongodb://localhost";
            var client = new MongoClient(conString);
            var server = client.GetServer();
            Database = server.GetDatabase(Settings.Default.FeelingsDatabase);
            Feelings.CreateIndex(IndexKeys<Feeling>.Ascending(x => x.Location));
        }


        public MongoCollection<ApplicationSetting> ApplicationSettings
        {
            get { return Database.GetCollection<ApplicationSetting>("ApplicationSettings"); }
        } 
        
        public MongoCollection<User> Users
        {
            get { return Database.GetCollection<User>("Users"); }
        } 
        
        public MongoCollection<Feeling> Feelings
        {
            get { return Database.GetCollection<Feeling>("Feelings"); }
        }
        
        public MongoCollection<Feel> Feels
        {
            get { return Database.GetCollection<Feel>("Feels"); }
        } 
        public MongoCollection<Response> Responses
        {
            get { return Database.GetCollection<Response>("Responses"); }
        }

    }

    public class Feel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Text { get; set; }

        public int Rank { get; set; }
    }
}