using System;
using System.Collections;
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
        }

        private IEnumerable<Feeling> AllFeelings()
        {
            return Context.Feelings.FindAll();
        }

        private IEnumerable<Feeling> FindFeelingsForUser(object username)
        {
            MongoCursor<Feeling> findFeelingsForUser = Context.Feelings.Find(Query.EQ("UserName", new BsonString(username.ToString())));
            return findFeelingsForUser;
        }

        private IEnumerable<Feeling> FindFeelings(object feel)
        {
            var feelText = feel.ToString();
            return Context.Feelings.Find(Query.EQ("FeelingTextLower", new BsonString(feelText.ToLower())));
        }

        private IEnumerable<Feeling> CreateFeeling()
        {
            var feeling = this.Bind<Feeling>();
            feeling.FeelingDate = DateTime.UtcNow;//.ToString("dd/MMM/yyyy HH:mm:ss");
            Context.Feelings.Insert(feeling);
            var allFeelings = FindFeelings(feeling.FeelingText).ToList();
            var currentFeeling = allFeelings.FirstOrDefault(f => f.Id == feeling.Id);
            allFeelings.Remove(currentFeeling);
            return allFeelings;
        }
    }
}