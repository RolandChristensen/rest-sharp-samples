using RestSharp;
using RestSharpLibrary.ResponseModels;
using System;

namespace RestSharpLibrary
{
    /// <summary>
    /// Class used when the service uses a token service to get short lived 
    /// tokens for authentication.
    /// </summary>
    public class TokenService
    {
        /// <summary>
        /// Used to retrieve a Bearer Token from the OAuth service. 
        /// You will need to set all of the properties in the TokenServiceProperties class to get a bearer token.
        /// </summary>
        /// <param name="environment">The environment under test.</param>
        /// <param name="tokenParams">The properties needed to get the token.
        /// </param>
        /// <returns>A tuple containing the Bearer Token and the number of 
        /// seconds before expiration.</returns>
        public static (string, int) GetTokenAndExpireTime(TokenProperties tokenParams)
        {
            if (string.IsNullOrWhiteSpace(tokenParams.Audience) || 
                string.IsNullOrWhiteSpace(tokenParams.ClientId) ||
                string.IsNullOrWhiteSpace(tokenParams.ClientSecret))
            {
                throw new ArgumentException("All of the parameters must be " +
                    "initialized to use this method.");
            }

            var options = new RestClientOptions(tokenParams.BaseUri);
            var client = new RestClient(options);

            var request = new RestRequest("oauth/token", Method.Post);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("audience", tokenParams.Audience);
            request.AddParameter("client_id", tokenParams.ClientId);
            request.AddParameter("client_secret", tokenParams.ClientSecret);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var response = client.ExecuteAsync<AuthToken>(request).Result;

            if (!response.IsSuccessful || response.Data == null)
            {
                throw new ApplicationException(
                    "The Authorization Service request was not successful" + Environment.NewLine +
                    $"Instead \"{response.StatusCode}\" was returned" + Environment.NewLine +
                    "With the JSON: " + Environment.NewLine + 
                    response.Content);
            }

            return (response.Data.Token, response.Data.ExpiresIn);
        }
    }
}
