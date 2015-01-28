using System;
using FeelKnitService.Serializers;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    public class Comment
    {
        [BsonSerializer(typeof(StringOrGuidSerializer))]
        public Guid Id { get; set; }

        public string Text { get; set; }

        public string User { get; set; }

        public string UserAvatar { get; set; }

        public DateTime PostedAt { get; set; }

        public bool IsReported { get; set; }
        
        public string ReportedBy{ get; set; }
        
        public DateTime ReportedAt{ get; set; }

        public bool IsDeleted { get; set; }

    }
}