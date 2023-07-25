using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RestSharpLibrary
{
    /// <summary>
    /// Class that only returns the deserialized response data.
    /// 
    /// This class throws exceptions when response is not a success or when
    /// RestSharp has an error.
    /// 
    /// Use exception handling to deal with failed responses.
    /// </summary>
    public class DeserializedResponseBase
    {
        /// <summary>
        /// The client used to execute requests against a service.
        /// </summary>
        public RestClient Client { get; }

        /// <summary>
        /// Use this for NTLM authentication, such as Azure DevOps 5.0.
        /// </summary>
        /// <param name="baseUri">The base URL of the service.</param>
        /// <param name="user">The username used to authorize requests.</param>
        /// <param name="pass">The password used to authorize requests.</param>
        /// <exception cref="ArgumentException">Thrown when parameters are not 
        /// set.</exception>
        public DeserializedResponseBase(Uri baseUri, string user, string pass)
        {
            if (baseUri == null)
                throw new ArgumentException("The base URI was not set.");
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException("The username was not set.");
            if (string.IsNullOrWhiteSpace(pass))
                throw new ArgumentException("The password was not set.");

            var options = new RestClientOptions(baseUri)
            {
                MaxTimeout = 40000,
                Credentials = new NetworkCredential(user, pass)
            };
            Client = new RestClient(options);
        }

        public DeserializedResponseBase(Uri baseUri)
        {
            var options = new RestClientOptions(baseUri)
            {
                MaxTimeout = 40000
            };
            Client = new RestClient(options)
            {
                Authenticator = new JwtAuthenticator("")
            };
        }

        public T Execute<T>(RestRequest request)
        {
            if (request == null) 
                throw new ArgumentException("The request was not set.");

            var response = Client.Execute<T>(request);

            return response.Data;
        }

        public async Task<T> ExecuteAsync<T>(RestRequest request)
        {
            if (request == null) 
                throw new ArgumentException("The request was not set.");

            var response = await Client.ExecuteAsync<T>(request);

            return response.Data;
        }
    }
}
