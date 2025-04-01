using System;

namespace UnionAvatars.API
{
    /// <summary>
    /// The session context contains data which should be a constant over the play session, or needs to be cached (such as the bodies array)
    /// </summary>
    public class SessionContext
    {
        private Uri url;
        public Uri Url { get => url; }
        private AuthToken userToken = null;
        public AuthToken UserToken { get => userToken; }
        private string organization = null;
        public string Organization { get => organization; }
        private string apiKey = null;
        public string ApiKey { get => apiKey; }

        public SessionContext(string urlString, string organization)
        {
            url = new Uri(urlString);
            this.organization = organization;
        }

        public void Authenticate (AuthToken token)
        {
            userToken = token;
        }

        public void AuthenticateWithApiKey (string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void Unauthenticate ()
        {
            userToken = null;
        }

        public void UnauthenticateApiKey ()
        {
            apiKey = null;
        }
    }
}