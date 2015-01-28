using System;
using System.Linq;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class EmailModule : BaseModule
    {
        public EmailModule()
            : base("/email")
        {
            
        }

    }
}