using System;
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
            return comment;
        }
    }
}