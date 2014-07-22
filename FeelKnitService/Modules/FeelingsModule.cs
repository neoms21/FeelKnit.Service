using System;
using System.Collections.Generic;
using System.Linq;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class FeelingsModule : BaseModule
    {
        public FeelingsModule()
            : base("/feelings")
        {
            Get["/"] = r => AllFeelings();
            Get["/{feel}"] = r => FindFeelings(r.feel);
            Get["/username/{username}"] = r => FindFeelingsForUser(r.username);

            Post["/"] = r => CreateFeeling();
            Post["/support/{feelingId}"] = r => IncreaseSupportCount(r.feelingId);
            Post["/createfeel/"] = r => CreateFeels();
        }

        private object IncreaseSupportCount(object feelingId)
        {
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
            feeling.SupportCount += 1;
            Context.Feelings.Save(feeling);
            return "Done";
        }

        private object CreateFeels()
        {
            var feeling = this.Bind<Feel>();
            Context.Feels.Insert(feeling);
            return feeling;
        }

        private IEnumerable<Feeling> AllFeelings()
        {
            return Context.Feelings.FindAll();
        }

        private IEnumerable<Feeling> FindFeelingsForUser(object username)
        {
            MongoCursor<Feeling> findFeelingsForUser = Context.Feelings.Find(Query.EQ("UserName", new BsonString(username.ToString())));
            return findFeelingsForUser.OrderByDescending(f => f.FeelingDate);
        }

        private IEnumerable<Feeling> FindFeelings(object feel)
        {
            var feelText = feel.ToString();
            return Context.Feelings.Find(Query.EQ("FeelingTextLower", new BsonString(feelText.ToLower())));
        }

        private IEnumerable<Feeling> CreateFeeling()
        {
            var feeling = this.Bind<Feeling>();
            feeling.FeelingDate = DateTime.UtcNow;//.AddDays(-GetRandom());//.ToString("dd/MMM/yyyy HH:mm:ss");
            Context.Feelings.Insert(feeling);
            var allFeelings = FindFeelings(feeling.FeelingText).ToList();
            var currentFeeling = allFeelings.FirstOrDefault(f => f.Id == feeling.Id);
            allFeelings.Remove(currentFeeling);
            return allFeelings;
        }


        private int GetRandom()
        {
            var rnd = new Random();
            return rnd.Next(0, 10);
        }
    }
}