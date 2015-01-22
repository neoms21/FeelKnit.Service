﻿using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class Comment
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Text { get; set; }

        public string User { get; set; }

        public string UserAvatar { get; set; }

        public DateTime PostedAt { get; set; }

    }
}