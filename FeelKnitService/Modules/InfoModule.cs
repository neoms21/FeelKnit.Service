namespace FeelKnitService.Modules
{
    public class InfoModule : BaseModule
    {
        public InfoModule() : base("/info")
        {
            Get["/"] = r => GetApplicationInfo();
        }

        private dynamic GetApplicationInfo()
        {
            return "From info";
            var applicationSetting = Context.ApplicationSettings.FindOne();
            return applicationSetting;
        }
    }
}