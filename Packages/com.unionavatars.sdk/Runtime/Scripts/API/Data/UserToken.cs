using Newtonsoft.Json;

namespace UnionAvatars.API
{
    public class AuthToken
    {
        [JsonProperty("token_type")]
        public string TokenType;
        [JsonProperty("access_token")]
        public string AccessToken;
    }
}