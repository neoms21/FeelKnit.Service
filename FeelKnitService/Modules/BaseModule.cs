using Nancy;

namespace FeelKnitService.Modules
{
    public abstract class BaseModule : NancyModule
    {
        protected readonly FeelingsContext Context = new FeelingsContext();

        public BaseModule(string path)
            : base(path)
        {

        }
    }
}