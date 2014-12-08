using JWT;

namespace FeelKnitService
{
    public interface IJwtWrapper
    {
        string Encode(object payload, string key, JwtHashAlgorithm algorithm);
    }

    public class JwtWrapper : IJwtWrapper
    {
        public string Encode(object payload, string key, JwtHashAlgorithm algorithm)
        {
            return JsonWebToken.Encode(payload, key, algorithm);
        }
    }
}