﻿using System.Linq;
using System.Security.Claims;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Nancy.TinyIoc;

namespace FeelKnitService
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            StaticConfiguration.DisableErrorTraces = false;
            var owinEnvironment = context.GetOwinEnvironment();
            var user = owinEnvironment["server.User"] as ClaimsPrincipal;
            if (user != null)
            {
                context.CurrentUser = new DemoUserIdentity
                {
                    UserName = user.Identity.Name,
                    Claims = user.Claims.Where(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(x => x.Value)
                };
            }

            pipelines.OnError.AddItemToEndOfPipeline((z, a) =>
            {
                return null;
            });
        }
    }
}