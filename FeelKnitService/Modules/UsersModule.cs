using System;
using System.Linq;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
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
            Post["/clientkey"] = r => SaveKey();
            Post["/clearkey"] = r => ClearKey();
        }

        private dynamic ClearKey()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.FindOne(Query<User>.EQ(u => u.UserName, user.UserName));
            dbUser.Key = string.Empty;
            Context.Users.Save(dbUser);
            return user;
        }

        private bool SaveKey()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.FindOne(Query<User>.EQ(u => u.UserName, user.UserName));
            dbUser.Key = user.Key;
            Context.Users.Save(dbUser);
            return true;
        }


        private bool VerfiyUser()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.Find(Query<User>.EQ(u => u.UserName, user.UserName)).FirstOrDefault();
            if (dbUser == null) return false;

            var hashedPassword = string.Format("sha1:{0}:{1}:{2}", PasswordHash.PBKDF2_ITERATIONS, dbUser.PasswordSalt, dbUser.Password);
            return PasswordHash.ValidatePassword(user.Password, hashedPassword);
        }

        private string CreateUser()
        {
            var user = this.Bind<User>();
            user.UserName = user.UserName.Trim();
            if (Context.Users.FindOne(Query.EQ("UserName", BsonValue.Create(user.UserName))) != null)
                return "Failure";
            var hashedPassword = PasswordHash.CreateHash(user.Password).Split(':');
            user.PasswordSalt = hashedPassword[2];
            user.Password = hashedPassword[3];
            Context.Users.Insert(user);
            return "true";
        }
    }
}