using FeelKnitService.Helpers;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace FeelKnitService.Modules
{
    public class EmailModule : BaseModule
    {
        public EmailModule()
            : base("/email")
        {
            Post["/report"] = r => ReportFeeling();
        }

        private object ReportFeeling()
        {
            var feelingId = Request.Form["feelingId"];
            var feeling = Context.Feelings.FindOne(Query.EQ("_id", new BsonObjectId(feelingId)));
            feeling.IsReported = true;
            Context.Feelings.Save(feeling);
            var username = Request.Form["username"];
            new EmailHelper().SendEmail(feelingId.ToString(), username.ToString());
            return null;
        }
    }
}