using System.Collections.Generic;
using Owin;
using Owin.StatelessAuth;

namespace FeelKnitService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)  
        {
            app.RequiresStatelessAuth(new TokenValidator(new ConfigProvider()),
                new StatelessAuthOptions { IgnorePaths = new List<string>(new[] { "/users/login", "/content/*.js" }) })
                .UseNancy();

        }
    }
}
