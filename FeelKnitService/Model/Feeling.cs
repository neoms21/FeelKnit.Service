﻿using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeelKnitService.Model
{
    //Comment for branch testing
    public class Feeling
    {
        private double _latitude;
        private double _longitude;
        private string _feelingText;

        public Feeling()
        {
            Location = new double[2];
            Comments = new List<Comment>();
            SupportUsers = new List<string>();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FeelingText
        {
            get { return _feelingText; }
            set
            {
                _feelingText = value;
                FeelingTextLower = value == null ? null : value.ToLower();
            }
        }

        public string FeelingTextLower { get; set; }

        public DateTime FeelingDate { get; set; }

        public string UserName { get; set; }

        public string UserAvatar { get; set; }

        // public User User { get; set; }

        public string Reason { get; set; }

        public string Action { get; set; }

        public int SupportCount { get; set; }

        public double[] Location { get; set; }

        public IList<Comment> Comments { get; set; }

        public IList<string> SupportUsers { get; set; }

        public bool IsReported { get; set; }

        public bool IsCurrentFeeling { get; set; }

        public DateTime ReportedAt { get; set; }

        public bool IsDeleted { get; set; }

        public double Latitude
        {
            get { return _latitude; }
            set
            {
                Location[1] = value;
                _latitude = value;
            }
        }

        public double Longitude
        {
            get { return _longitude; }
            set
            {
                Location[0] = value;
                _longitude = value;
            }
        }

        public string ReportedBy { get; set; }
        public int CommentsCount { get; set; }
    }
}