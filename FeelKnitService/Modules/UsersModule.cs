using System.Collections.Generic;
using System.Linq;
using FeelKnitService.Model;
using MongoDB.Driver.Builders;
using Nancy;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class UsersModule : BaseModule
    {
        public UsersModule()
            : base("/users")
        {
            Get["/"] = r => new User { UserName = "Manoj" };
            
            Post["/"] = r => CreateUser();
            Post["/Verify"] = r => VerfiyUser();
        }


        private bool VerfiyUser()
        {
            var user = this.Bind<User>();

            var dbUser = Context.Users.Find(Query<User>.EQ(u => u.UserName, user.UserName)).FirstOrDefault();
            if (dbUser != null && dbUser.Password.Equals(user.Password))
                return true;

            return false;
        }

        private User CreateUser()
        {

            var user = this.Bind<User>();
            var x = Context.Users.Insert(user);
            return user;
        }
    }
}