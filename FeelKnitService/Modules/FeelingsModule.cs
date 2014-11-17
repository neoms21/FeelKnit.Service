using System;
using System.Collections.Generic;
using System.Linq;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class FeelingsModule : BaseModule
    {
        public FeelingsModule()
            : base("/feelings")
        {
            Get["/"] = r => AllFeelings();
            //Get["/relatedfeelings"] = r => FindFeelings();
            Get["/userfeelings"] = r => FindUserFeeling();
            Get["/username/{username}"] = r => FindFeelingsForUser(r.username);
            Get["/comments/{username}"] = r => FindFeelingsForCommentsUser(r.username);
            Get["/getfeels"] = r => Fetchfeels();

            Post["/"] = r => CreateFeeling();
            Post["/increasesupport"] = r => IncreaseSupportCount();
            Post["/decreasesupport"] = r => DecreaseSupportCount();
            Post["/createfeel/"] = r => CreateFeels();
            //Post["/updatefeelings"] = r => UpdateFeelings();
        }

        private IEnumerable<string> Fetchfeels()
        {
            var feels = Context.Feels.AsQueryable();
            return feels.OrderBy(x => x.Rank).Select(x => x.Text);
        }

        //private bool UpdateFeelings()
        //{
        //    var feelings = Context.Feelings.FindAll().OrderByDescending(f => f.FeelingDate);

        //    var groupedfeelings = feelings.GroupBy(f => f.UserName);

        //    foreach (var groupedfeeling in groupedfeelings)
        //    {
        //        var i = 0; ;
        //        foreach (var feeling in groupedfeeling)
        //        {
        //            feeling.IsCurrentFeeling = i == 0;
        //            i++;
        //            Context.Feelings.Save(feeling);
        //        }
        //    }
        //    return true;
        //}

        private Feeling FindUserFeeling()
        {
            var feelingText = Request.Query["feeling"].ToString().ToLower();
            var username = Request.Query["username"];
            var query = Query.And(Query.EQ("FeelingTextLower", new BsonString(feelingText)),
               Query.EQ("UserName", new BsonString(username)));


            Feeling findUserFeeling = Context.Feelings.Find(query).SetSortOrder(SortBy.Descending("feelingDate")).FirstOrDefault();
            return findUserFeeling;
        }

        private IEnumerable<Feeling> FindFeelingsForCommentsUser(object username)
        {
            var commentQuery = Query<Comment>.EQ(pr => pr.User, Convert.ToString(username));
            var finalQuery = Query<Feeling>.ElemMatch(p => p.Comments, builder => commentQuery);

            return Context.Feelings.Find(finalQuery).OrderByDescending(f => f.FeelingDate);
        }

        private object IncreaseSupportCount()
        {
            var feelingId = Request.Form["feelingId"];
            string username = Request.Form["username"].ToString();
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
            var modUpdate = Update<Feeling>.Push(f => f.SupportUsers, username);
            feeling.SupportCount += 1;
            Context.Feelings.Save(feeling);
            Context.Feelings.Update(Query.EQ("_id", new ObjectId(feelingId)), modUpdate);
            return "Done";
        }

        private object DecreaseSupportCount()
        {
            var feelingId = Request.Form["feelingId"];
            string username = Request.Form["username"].ToString();
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
            var modUpdate = Update<Feeling>.Pull(f => f.SupportUsers, username);
            feeling.SupportCount -= 1;
            Context.Feelings.Save(feeling);
            Context.Feelings.Update(Query.EQ("_id", new ObjectId(feelingId)), modUpdate);
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
            var findFeelingsForUser = Context.Feelings.Find(Query.EQ("UserName", new BsonString(username.ToString())));
            return findFeelingsForUser.OrderByDescending(f => f.FeelingDate);
        }

        private IEnumerable<Feeling> FindFeelings(Feeling feeling)
        {
            var query = Query.And(Query.EQ("FeelingTextLower", new BsonString(feeling.FeelingTextLower)),
                Query.EQ("IsCurrentFeeling", new BsonBoolean(true)),
                Query.NE("UserName", new BsonString(feeling.UserName)));

            var relatedFeelings = Context.Feelings.Find(query);
            var groupedFeelings = relatedFeelings.OrderByDescending(f => f.FeelingDate).GroupBy(f => f.UserName);
            var finalFeelings = groupedFeelings.Select(groupedFeeling => groupedFeeling.First()).ToList();

            return finalFeelings;
        }

        //private IEnumerable<Feeling> FindFeelings()
        //{
        //    var feeling = Request.Query.feeling;
        //    var username = Request.Query.username;
        //    var query = Query.And(Query.EQ("FeelingTextLower", new BsonString(feeling)), Query.NE("UserName", new BsonString(username)));

        //    return Context.Feelings.Find(query).SetSortOrder(SortBy.Descending("feelingDate"));
        //  }

        private IEnumerable<Feeling> CreateFeeling()
        {

            var feeling = this.Bind<Feeling>();
            var query = Query.And(Query.EQ("UserName", new BsonString(feeling.UserName)), Query.EQ("IsCurrentFeeling", new BsonBoolean(true)));
            var update = Update.Set("IsCurrentFeeling", new BsonBoolean(false));
            Context.Feelings.Update(query, update, UpdateFlags.Multi);
            feeling.FeelingDate = DateTime.UtcNow;
            feeling.IsCurrentFeeling = true;
            var dbUser = Context.Users.FindOne(Query.EQ("UserName", new BsonString(feeling.UserName)));
            feeling.User = dbUser;
            Context.Feelings.Insert(feeling);
            var allFeelings = FindFeelings(feeling).ToList();
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