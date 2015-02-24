using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeelKnitService.Helpers;
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
            Get["/{feelingId}"] = r => GetFeelingById(r.feelingId);
            Get["/userfeelings"] = r => FindUserFeeling();
            Get["/username/{username}"] = r => FindFeelingsForUser(r.username);
            Get["/comments/{username}"] = r => FindFeelingsForCommentsUser(r.username);
            Get["/relatedfeelings/{username}"] = r => FindRelatedFeelingsForUser(r.username);
            Get["/getfeels"] = r => Fetchfeels();

            Post["/"] = r => CreateFeeling();
            Post["/increasesupport"] = r => IncreaseSupportCount();
            Post["/decreasesupport"] = r => DecreaseSupportCount();
            Post["/report"] = r => ReportFeeling();
            Post["/createfeel/"] = r => CreateFeels();
        }

        private IEnumerable<Feeling> FindRelatedFeelingsForUser(object username)
        {
            var feeling =
                Context.Feelings.FindOne(Query.And(Query.EQ("UserName", new BsonString(username.ToString())),
                    Query.EQ("IsCurrentFeeling", new BsonBoolean(true))));
            if (feeling == null) return null;

            var relatedFeelings = FindFeelings(feeling.FeelingTextLower, feeling.UserName).ToList();
            relatedFeelings.Insert(0, feeling);
            RemoveDeletedComments(relatedFeelings);
            AddUserAvatar(relatedFeelings);
            return relatedFeelings;
        }

        private IEnumerable<string> Fetchfeels()
        {
            // var push = new PushNotificationApple();
            var feels = Context.Feels.AsQueryable();
            return feels.OrderBy(x => x.Text).Select(x => x.Text);
        }

        private Feeling FindUserFeeling()
        {
            var feelingText = Request.Query["feeling"].ToString().ToLower();
            var username = Request.Query["username"];
            var query = Query.And(Query.EQ("FeelingTextLower", new BsonString(feelingText)),
               Query.EQ("UserName", new BsonString(username)));


            Feeling findUserFeeling = Context.Feelings.Find(query).SetSortOrder(SortBy.Descending("feelingDate")).FirstOrDefault();
            AddUserAvatar(new List<Feeling> { findUserFeeling });
            return findUserFeeling;
        }

        private IEnumerable<Feeling> FindFeelingsForCommentsUser(object username)
        {
            var commentQuery = Query<Comment>.EQ(pr => pr.User, Convert.ToString(username));
            var finalQuery = Query<Feeling>.ElemMatch(p => p.Comments, builder => commentQuery);

            var findFeelingsForCommentsUser = Context.Feelings.Find(finalQuery).OrderByDescending(f => f.FeelingDate).ToList();
            AddUserAvatar(findFeelingsForCommentsUser);
            return findFeelingsForCommentsUser;
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

        private Feeling GetFeelingById(dynamic id)
        {
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(id.ToString()))));
            AddUserAvatar(new List<Feeling> { feeling });
            return feeling;
        }

        private IEnumerable<Feeling> FindFeelingsForUser(object username)
        {
            var finalQuery = Query.And(Query.EQ("UserName", new BsonString(username.ToString())), Query.NE("IsDeleted", new BsonBoolean(true)));

            var findFeelingsForUser = Context.Feelings.Find(finalQuery);
            var feelings = findFeelingsForUser.OrderByDescending(f => f.FeelingDate).ToList();
            RemoveDeletedComments(feelings);
            AddUserAvatar(feelings);
            return feelings;
        }

        private IEnumerable<Feeling> FindFeelings(string feelingText, string username)
        {
            var query = Query.And(Query.EQ("FeelingTextLower", new BsonString(feelingText)),
                Query.EQ("IsCurrentFeeling", new BsonBoolean(true)),
                Query.NE("UserName", new BsonString(username)), Query.NE("IsDeleted", new BsonBoolean(true)));

            var relatedFeelings = Context.Feelings.Find(query);
            var groupedFeelings = relatedFeelings.OrderByDescending(f => f.FeelingDate).GroupBy(f => f.UserName);
            var finalFeelings = groupedFeelings.Select(groupedFeeling => groupedFeeling.First()).ToList();

            return finalFeelings;
        }

        private IEnumerable<Feeling> CreateFeeling()
        {
            var feeling = this.Bind<Feeling>();
            var query = Query.And(Query.EQ("UserName", new BsonString(feeling.UserName)), Query.EQ("IsCurrentFeeling", new BsonBoolean(true)));
            var update = Update.Set("IsCurrentFeeling", new BsonBoolean(false));
            Context.Feelings.Update(query, update, UpdateFlags.Multi);
            feeling.FeelingDate = DateTime.UtcNow;
            feeling.IsCurrentFeeling = true;
            var dbUser = Context.Users.FindOne(Query.EQ("UserName", new BsonString(feeling.UserName)));
            feeling.UserAvatar = dbUser.Avatar;
            Context.Feelings.Insert(feeling);
            var allFeelings = FindFeelings(feeling.FeelingTextLower, feeling.UserName).ToList();
            var currentFeeling = allFeelings.FirstOrDefault(f => f.Id == feeling.Id);
            allFeelings.Remove(currentFeeling);
            RemoveDeletedComments(allFeelings);
            AddUserAvatar(allFeelings);
            return allFeelings;
        }

        private dynamic ReportFeeling()
        {
            var feelingId = Request.Form["feelingId"];
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(feelingId)));
            feeling.IsReported = true;
            feeling.ReportedAt = DateTime.UtcNow;
            Context.Feelings.Save(feeling);
            var reportedBy = Request.Form["username"];
            Task.Run(() => EmailHelper.SendEmail("Feeling Reported!!", string.Format("FeelingId {0} of user {1} has been reported by {2}", feelingId, feeling.UserName, reportedBy)));
            return null;
        }

        private void RemoveDeletedComments(IEnumerable<Feeling> feelings)
        {
            foreach (var feeling in feelings)
            {
                var comments = feeling.Comments.Where(c => !c.IsDeleted).ToList();
                feeling.Comments.Clear();
                feeling.Comments = comments.ToList();
            }
        }

        private object CreateFeels()
        {
            var feeling = this.Bind<Feel>();
            if (string.IsNullOrWhiteSpace(feeling.Text))
                return null;
            Context.Feels.Insert(feeling);
            return feeling;
        }

        private void AddUserAvatar(List<Feeling> feelings)
        {
            feelings.ForEach(f =>
            {
                var dbUser = Context.Users.FindOne(Query.EQ("UserName", new BsonString(f.UserName)));
                f.UserAvatar = dbUser.Avatar;
                f.Comments.ForEach(c =>
                {
                    var dbUserForComment = Context.Users.FindOne(Query.EQ("UserName", new BsonString(c.User)));
                    c.UserAvatar = dbUserForComment.Avatar;
                });
            });
        }

    }
}