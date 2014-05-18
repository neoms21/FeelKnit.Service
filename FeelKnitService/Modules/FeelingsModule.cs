using System;
using System.Collections.Generic;
using System.Linq;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class FeelingsModule : BaseModule
    {
        public FeelingsModule()
            : base("/feelings")
        {
            Get["/{feel}"] = r => FindFeelings(r.feel);
            Post["/"] = r => CreateFeeling();
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
            var allFeelings =  FindFeelings(feeling.FeelingText).ToList();
            var currentFeeling = allFeelings.FirstOrDefault(f => f.Id == feeling.Id);
            allFeelings.Remove(currentFeeling);
            return allFeelings;
        }
    }
}