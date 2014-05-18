﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FeelKnitService.Model;
using FeelKnitService.Properties;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace FeelKnitService
{
    
    public class FeelingsContext
    {

        public MongoDatabase Database;

        public FeelingsContext()
        {
            var client = new MongoClient(Settings.Default.MONGOHQ_URL);
            var server = client.GetServer();
            Database = server.GetDatabase(Settings.Default.FeelingsDatabase);
            Feelings.CreateIndex(IndexKeys<Feeling>.Ascending(x => x.Location));
        }


        public MongoCollection<User> Users
        {
            get { return Database.GetCollection<User>("Users"); }
        } 
        
        public MongoCollection<Feeling> Feelings
        {
            get { return Database.GetCollection<Feeling>("Feelings"); }
        }

    }
}