﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class ApplicationSetting
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public bool FeelingsUpdated { get; set; }
        public bool NewVersionAvailable { get; set; }
        public string VersionName { get; set; }
    }
}