using System;
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
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
            var user = Context.Users.FindAll().First(u => u.UserName.Equals(feeling.UserName));
            Context.Feelings.Update(Query.EQ("_id", new ObjectId(feelingId)), modUpdate);
            Task.Factory.StartNew(() => SendNotification(comment.User, user.Key));

            return comment;
        }

        private void SendNotification(string user, string key)
        {
            new GcmService().SendRequest(user, key);
        }
    }
}