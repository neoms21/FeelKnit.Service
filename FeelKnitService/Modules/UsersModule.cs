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
            Post["/forgot"] = r => ForgotPassword();
            Post["/updateEmail"] = r => UpdateEmail();
            Post["/updatePassword"] = r => UpdatePassword();
            Post["/saveAvatar"] = r => UpdateUserAvatar();
        }

        private bool UpdateUserAvatar()
        {
            var user = this.Bind<User>();
            var dbUser = GetUser(user.UserName);
            dbUser.Avatar = user.Avatar;
            Context.Users.Save(dbUser);
            return true;
        }

        private bool UpdatePassword()
        {
            var user = this.Bind<User>();
            var dbUser = GetUser(user.UserName);
            SetHashedPassword(dbUser, user.Password);
            dbUser.IsTemporary = false;
            dbUser.PasswordExpiryTime = null;
            Context.Users.Save(dbUser);
            return true;
        }

        private object UpdateEmail()
        {
            var username = Request.Form["username"].ToString();
            User user = GetUser(username);
            user.EmailAddress = Request.Form["email"].ToString();
            Context.Users.Save(user);
            GeneratePassword(user);
            return user;
        }

        private object ForgotPassword()
        {
            var username = Request.Form["username"].ToString();
            User user = GetUser(username);
            GeneratePassword(user);
            return user;
        }

        private void GeneratePassword(User user)
        {
            var randomPassword = GetRandomPassword();
            user.Password = randomPassword;
            SetHashedPassword(user);
            user.PasswordExpiryTime = DateTime.UtcNow.AddHours(2);
            user.IsTemporary = true;
            Context.Users.Save(user);
            EmailHelper.Send("Password Recovery", string.Format("Your new password is {0}", randomPassword), user.EmailAddress);
        }

        private string GetRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        private dynamic ClearKey()
        {
            var user = this.Bind<User>();
            var dbUser = GetUser(user.UserName);
            dbUser.Key = string.Empty;
            Context.Users.Save(dbUser);
            return user;
        }

        private User GetUser(string userName)
        {
            return Context.Users.FindOne(Query<User>.EQ(u => u.UserName, userName));
        }

        private bool SaveKey()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.FindOne(Query<User>.EQ(u => u.UserName, user.UserName));
            dbUser.Key = user.Key;
            Context.Users.Save(dbUser);
            return true;
        }

        private dynamic VerfiyUser()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.Find(Query<User>.EQ(u => u.UserName, user.UserName)).FirstOrDefault();
            if (dbUser == null) return false;

            var hashedPassword = string.Format("sha1:{0}:{1}:{2}", PasswordHash.PBKDF2_ITERATIONS, dbUser.PasswordSalt, dbUser.Password);
            bool isValidPassword = PasswordHash.ValidatePassword(user.Password, hashedPassword);
            return isValidPassword ? new { IsLoginSuccessful = true, dbUser.Avatar } : new { IsLoginSuccessful = false, Avatar = string.Empty };
            //var isValidPassword = PasswordHash.ValidatePassword(user.Password, hashedPassword) &&
            //    (dbUser.PasswordExpiryTime == null || DateTime.UtcNow < dbUser.PasswordExpiryTime);
            //return new UserVerification { IsTemporary = dbUser.IsTemporary, IsValid = isValidPassword };
        }

        private string CreateUser()
        {
            var user = this.Bind<User>();
            user.UserName = user.UserName.Trim();
            if (Context.Users.FindOne(Query.EQ("UserName", BsonValue.Create(user.UserName))) != null)
                return "Failure";
            SetHashedPassword(user);
            Context.Users.Insert(user);
            return "true";
        }

        private static void SetHashedPassword(User user, string password = "")
        {
            var hashedPassword = PasswordHash.CreateHash(string.IsNullOrEmpty(password) ? user.Password : password).Split(':');
            user.PasswordSalt = hashedPassword[2];
            user.Password = hashedPassword[3];
        }
    }
}