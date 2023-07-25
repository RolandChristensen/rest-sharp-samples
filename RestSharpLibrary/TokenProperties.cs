using System;

namespace RestSharpLibrary
{
    /// <summary>
    /// These properties contain the values needed for the request parameters 
    /// to get a token from the token service.
    /// </summary>
    public class TokenProperties
    {
        public Uri BaseUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Audience { get; set; }
    }
}
