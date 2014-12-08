using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using JWT;
using Owin.StatelessAuth;

namespace FeelKnitService
{
    public class TokenValidator : ITokenValidator
    {
        private readonly IConfigProvider _configProvider;

        public TokenValidator(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public ClaimsPrincipal ValidateUser(string token)
        {
            try
            {
                //Claims don't deserialize :(
                //var jwttoken = JsonWebToken.DecodeToObject<JwtToken>(token, configProvider.GetAppSetting("securekey"));

                var decodedtoken = JsonWebToken.DecodeToObject(token, _configProvider.GetAppSetting("securekey")) as Dictionary<string, object>;
                if (decodedtoken == null)
                    return null;

                var jwttoken = new JwtToken()
                {
                    Audience = (string)decodedtoken["Audience"],
                    Issuer = (string)decodedtoken["Issuer"],
                    Expiry = DateTime.Parse(decodedtoken["Expiry"].ToString()),
                };

                if (decodedtoken.ContainsKey("Claims"))
                {
                    var claims = new List<Claim>();

                    for (int i = 0; i < ((ArrayList)decodedtoken["Claims"]).Count; i++)
                    {
                        var type = ((Dictionary<string, object>)((ArrayList)decodedtoken["Claims"])[i])["Type"].ToString();
                        var value = ((Dictionary<string, object>)((ArrayList)decodedtoken["Claims"])[i])["Value"].ToString();
                        claims.Add(new Claim(type, value));
                    }

                    jwttoken.Claims = claims;
                }

                if (jwttoken.Expiry < DateTime.UtcNow)
                {
                    return null;
                }

                //TODO Tidy on 3.8 Mono release
                var claimsPrincipal = new ClaimsPrincipal();
                var claimsIdentity = new ClaimsIdentity("Token");
                claimsIdentity.AddClaims(jwttoken.Claims);
                claimsPrincipal.AddIdentity(claimsIdentity);
                return claimsPrincipal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
