using FeelKnitService.Model;
using Nancy;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class UsersModule : BaseModule
    {
        public UsersModule() : base("/users")
        {
            Get["/"] = r => new User { UserName = "Manoj" };
            Post["/"] = r => CreateUser();
        }

        private User CreateUser()
        {
            var user = this.Bind<User>();
            var x = Context.Users.Insert(user);
            return user;
        }
    }
}