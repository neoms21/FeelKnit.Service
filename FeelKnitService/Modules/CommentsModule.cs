using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Nancy;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class CommentsModule : BaseModule
    {
        public CommentsModule()
            : base("/comments")
        {
            Post["/{feelingId}"] = r => AddComment(r.feelingId);
        }

        private object AddComment(dynamic feelingId)
        {
            //var feeling = Context.Feelings.FindOneById(new ObjectId(feelingId));
            var comment = this.Bind<Comment>();
            comment.PostedAt = DateTime.UtcNow;
            var modUpdate = Update<Feeling>.Push(p => p.Comments, comment);


            Context.Feelings.Update(Query.EQ("_id", new ObjectId(feelingId)), modUpdate);
            Task.Factory.StartNew(() =>
            {
                var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
                var commentUsers = feeling.Comments.Select(c => c.User).Where(x => !x.Equals(feeling.UserName)).ToList();
                var bsonValues = new List<BsonValue>();
                commentUsers.ForEach(c => bsonValues.Add(BsonValue.Create(c)));
                var users = Context.Users.Find(Query.In("UserName", bsonValues)).ToList();//.First(u => u.UserName.Equals(feeling.UserName));
                SendNotification(feeling,comment.User, users);
            });

            return comment;
        }

        private void SendNotification(Feeling feeling, string user, List<User> users)
        {
            new GcmService().SendRequest(feeling,user, users);
        }
    }
}