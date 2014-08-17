using FeelKnitService.Helpers;

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
            var feeling = Request.Form["feelingId"];
            var username = Request.Form["username"];
            new EmailHelper().SendEmail(feeling.ToString(), username.ToString());
            return null;
        }
    }
}