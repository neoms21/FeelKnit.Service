using System;
using Nancy;
using Nancy.Json;

namespace FeelKnitService.Modules
{
    public abstract class BaseModule : NancyModule
    {
        protected new readonly FeelingsContext Context = new FeelingsContext();

        protected BaseModule(string path)
            : base(path)
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;
        }
    }
}