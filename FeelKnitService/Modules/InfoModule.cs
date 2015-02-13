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
            var applicationSetting = Context.ApplicationSettings.FindOne();
            return applicationSetting;
        }
    }
}