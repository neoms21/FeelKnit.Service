using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using JWT;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Nancy;
using Nancy.ModelBinding;

namespace FeelKnitService.Modules
{
    public class UsersModule : BaseModule
    {
        private readonly IConfigProvider _configProvider;
        private readonly IJwtWrapper _jwtWrapper;

        public UsersModule(IConfigProvider configProvider, IJwtWrapper jwtWrapper)
            : base("/users")
        {
            _configProvider = configProvider;
            _jwtWrapper = jwtWrapper;

            Post["/"] = r => CreateUser();
            Post["/login"] = r => Login();
            Post["/clientkey"] = r => SaveKey();
            Post["/devicetoken"] = r => SaveAppleToken();
            Post["/clearkey"] = r => ClearKey();
            Post["/forgot"] = r => ForgotPassword();
            Post["/updateEmail"] = r => UpdateEmail();
            Post["/updatePassword"] = r => UpdatePassword();
            Post["/saveAvatar"] = r => UpdateUserAvatar();
            Post["/saveuser"] = r => UpdateUserProfile();
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

        private bool SaveAppleToken()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.FindOne(Query<User>.EQ(u => u.UserName, user.UserName));
            dbUser.IosKey = user.IosKey;
            Context.Users.Save(dbUser);
            return true;
        }

        private dynamic Login()
        {
            var user = this.Bind<User>();
            var dbUser = Context.Users.Find(Query<User>.EQ(u => u.UserNameLower, user.UserName.ToLowerInvariant())).FirstOrDefault();
            if (dbUser == null) return new { IsLoginSuccessful = false };

            var hashedPassword = string.Format("sha1:{0}:{1}:{2}", PasswordHash.PBKDF2_ITERATIONS, dbUser.PasswordSalt, dbUser.Password);
            var isValidPassword = PasswordHash.ValidatePassword(user.Password, hashedPassword);

            if (!isValidPassword)
                return new { IsLoginSuccessful = false, Avatar = string.Empty };

            var token = GenerateAuthorizationToken(user.UserName);
            Negotiate.WithModel(token);
            return new { IsLoginSuccessful = true, dbUser.Avatar, Token = token, UserEmail = dbUser.EmailAddress };
            //var isValidPassword = PasswordHash.ValidatePassword(user.Password, hashedPassword) &&
            //    (dbUser.PasswordExpiryTime == null || DateTime.UtcNow < dbUser.PasswordExpiryTime);
            //return new UserVerification { IsTemporary = dbUser.IsTemporary, IsValid = isValidPassword };
        }


        private dynamic CreateUser()
        {
            var user = this.Bind<User>();
            user.UserName = user.UserName.Trim();
            if (Context.Users.FindOne(Query.EQ("UserName", BsonValue.Create(user.UserName))) != null)
                return new { IsLoginSuccessful = false, Error = "Username is not unique." };

            SetHashedPassword(user);
            Context.Users.Insert(user);
            var token = GenerateAuthorizationToken(user.UserName);
            return new { IsLoginSuccessful = true, Token = token, UserEmail = user.EmailAddress };
        }

        private static void SetHashedPassword(User user, string password = "")
        {
            var hashedPassword = PasswordHash.CreateHash(string.IsNullOrEmpty(password) ? user.Password : password).Split(':');
            user.PasswordSalt = hashedPassword[2];
            user.Password = hashedPassword[3];
        }

        private string GenerateAuthorizationToken(string username)
        {
            var jwttoken = new JwtToken
            {
                Issuer = "http://feelknit.com",
                Audience = "http://feelknit-audience.com",
                Claims =
                    new List<Claim>(new[]
                    {
                        new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User"),
                        new Claim(ClaimTypes.Name, username)
                    }),
                Expiry = DateTime.UtcNow.AddYears(10)
            };

            var token = _jwtWrapper.Encode(jwttoken, _configProvider.GetAppSetting("securekey"), JwtHashAlgorithm.HS256);
            return token;
        }

        private bool UpdateUserProfile()
        {
            var user = this.Bind<User>();
            var dbUser = GetUser(user.UserName);
            dbUser.Avatar = user.Avatar;
            dbUser.EmailAddress = user.EmailAddress;
            Context.Users.Save(dbUser);
            return true;
        }
    }
}