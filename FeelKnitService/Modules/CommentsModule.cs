﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Nancy.ModelBinding;
using Response = FeelKnitService.Model.Response;

namespace FeelKnitService.Modules
{
    public class CommentsModule : BaseModule
    {
        public CommentsModule()
            : base("/comments")
        {
            Post["/{feelingId}", true] = async (r, c) => await AddComment(r.feelingId);
            Post["/report"] = r => ReportComment();
        }

        private async Task<object> AddComment(dynamic feelingId)
        {
            //var feeling = Context.Feelings.FindOneById(new ObjectId(feelingId));
            var comment = this.Bind<Comment>();
            var user = Context.Users.FindOne(Query.EQ("UserName", new BsonString(comment.User)));
            comment.UserAvatar = user.Avatar;
            comment.PostedAt = DateTime.UtcNow;
            comment.Id = Guid.NewGuid();

            var modUpdate1 = Update<Feeling>.Push(p => p.Comments, comment);

            Context.Feelings.Update(Query.EQ("_id", new ObjectId(feelingId)), modUpdate1);
            await Task.Factory.StartNew(() => SendNotification(feelingId, comment));
            return comment;
        }

        private dynamic SendNotification(dynamic feelingId, Comment comment)
        {

            var feeling =
                Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(new ObjectId(feelingId.ToString()))));
            var feelingUser = Context.Users.FindOne(Query.EQ("UserName", new BsonString(feeling.UserName))); //prits
            var commentUsers =
                feeling.Comments.Select(c => c.User)
                    .Where(x => x != feeling.UserName && x != comment.User)
                    .Distinct()
                    .ToList(); //neo
            var bsonValues = new List<BsonValue>();
            commentUsers.ForEach(c => bsonValues.Add(BsonValue.Create(c)));
            var users = Context.Users.Find(Query.In("UserName", bsonValues)).ToList();
            //.First(u => u.UserName.Equals(feeling.UserName));
            SendNotification(feeling, comment, users, feelingUser);
            return true;
        }

        private object ReportComment()
        {
            var comment = this.Bind<Comment>();
            var feelingId = Request.Form["feelingId"];
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(feelingId)));
            var commentToReport = feeling.Comments.First(c => c.Id == comment.Id);
            commentToReport.IsReported = true;
            commentToReport.ReportedBy = comment.ReportedBy;
            commentToReport.ReportedAt = DateTime.UtcNow;

            Context.Feelings.Save(feeling);
            Task.Run(() => EmailHelper.SendEmail("Comment Reported!!", string.Format("CommentId - {0} under FeelingId {1} has been reported by {2}",
               commentToReport.Id, feelingId, comment.ReportedBy)));
            return null;
        }

        private void SendNotification(Feeling feeling, Comment comment, List<User> users, User feelingUser)
        {
            try
            {
                new PushNotificationService().SendCommentNotifications(feeling, comment, users, SaveResponse, feelingUser);
            }
            catch (Exception e)
            {
                LogWriter.Write(e.ToString());
            }
        }

        private void SaveResponse(string response)
        {
            Context.Responses.Insert(new Response { ResponseText = response, TimeRecorded = DateTime.UtcNow });
        }
    }
}