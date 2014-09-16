﻿using System;
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
            Get["/"] = r => AllFeelings();
            //Get["/relatedfeelings"] = r => FindFeelings();
            Get["/userfeelings"] = r => FindUserFeeling();
            Get["/username/{username}"] = r => FindFeelingsForUser(r.username);
            Get["/comments/{username}"] = r => FindFeelingsForCommentsUser(r.username);

            Post["/"] = r => CreateFeeling();
            Post["/increasesupport"] = r => IncreaseSupportCount();
            Post["/decreasesupport"] = r => DecreaseSupportCount();
            Post["/createfeel/"] = r => CreateFeels();
        }

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
            feeling.FeelingDate = DateTime.UtcNow;//.AddDays(-GetRandom());//.ToString("dd/MMM/yyyy HH:mm:ss");
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