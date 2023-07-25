using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace RestSharpLibrary
{
    /// <summary>
    /// Class that returns the RestResponse. 
    /// Enables you to examine the status code, and any errors or exceptions 
    /// from the class that inherits this one.
    /// Avoid throwing exceptions from this class other than ones that flag
    /// coding errors, such as ArgumentException.
    /// </summary>
    public class MultiThreadedBase
    {
        private TokenProperties TokenParameters { get; set; }

        private string BearerToken { get; set; }

        private Timer TokenTimer { get; set; }

        /// <summary>
        /// The client used to execute requests against a service.
        /// </summary>
        private RestClient Client { get; }

        /// <summary>
        /// Used to initialize the class with only a base URL and no 
        /// authentication. 
        /// Example usage is for the GitHub API public side of things.
        /// </summary>
        /// <param name="baseUri">The base URL of the API.</param>
        public MultiThreadedBase(Uri baseUri)
        {
            var options = new RestClientOptions(baseUri)
            {
                MaxTimeout = 40000
            };
            Client = new RestClient(options);
        }

        /// <summary>
        /// Used to initialize the class with only a base URL and an access 
        /// token. 
        /// Example usage is for the GitHub API public side of things, where 
        /// you would pass in a Personal Access Token.
        /// </summary>
        /// <param name="baseUri">The base URL of the API.</param>
        /// <param name="authToken">The token used in the authorization header.
        /// </param>
        public MultiThreadedBase(Uri baseUri, string authToken)
        {
            if (string.IsNullOrWhiteSpace(authToken))
                throw new ArgumentException("The token was not set.");

            var options = new RestClientOptions(baseUri)
            {
                MaxTimeout = 40000
            };
            Client = new RestClient(options);
            Client.AddDefaultHeader("Authorization", "token " + authToken);
        }

        /// <summary>
        /// Use this for NTLM authentication.
        /// I use this method for the Azure DevOps 5.0 API.
        /// </summary>
        /// <param name="baseUri">The base URL of the service.</param>
        /// <param name="user">The username used to authorize requests.</param>
        /// <param name="pass">The password used to authorize requests.</param>
        /// <exception cref="ArgumentException">Thrown when parameters are not 
        /// set.</exception>
        public MultiThreadedBase(Uri baseUri, string user, string pass)
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

        /// <summary>
        /// use this for Bearer Token based authentication, such as Auth0.
        /// </summary>
        /// <param name="baseUri">The base URI of the service.</param>
        /// <param name="tokenProperties">The TokenServiceParams object initialized
        /// with the credentials needed to get a token from the token service.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if any of the parameters
        /// are not initialized. That includes the properties of the 
        /// TokenServiceParams object.</exception>
        public MultiThreadedBase(Uri baseUri, TokenProperties tokenProperties)
        {
            if (string.IsNullOrWhiteSpace(tokenProperties.ClientId))
                throw new ArgumentException("The client ID was not set.");
            if (string.IsNullOrWhiteSpace(tokenProperties.ClientSecret))
                throw new ArgumentException("The client secret was not set.");
            if (string.IsNullOrWhiteSpace(tokenProperties.Audience))
                throw new ArgumentException("The audience was not set.");

            TokenParameters = tokenProperties;

            SetBearerToken();

            var options = new RestClientOptions(baseUri);
            Client = new RestClient(options)
            {
                Authenticator = new JwtAuthenticator(BearerToken)
            };
        }

        /// <summary>
        /// Executes the request passed, but does not deserialize the response.
        /// 
        /// This can be useful for getting a response from the service before
        /// knowing what the response looks like. You can then use the raw 
        /// content to create a class to deserialize the response.
        /// </summary>
        /// <param name="request">The request to execute.</param>
        /// <returns>The RestResponse object.</returns>
        /// <exception cref="ArgumentException">Thrown if the parameters are 
        /// not set.</exception>
        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            if (request == null) 
                throw new ArgumentException("The request was not set.");

            var response = await Client.ExecuteAsync(request);
            LogTransaction(request, response);

            return response;
        }

        /// <summary>
        /// Executes the response passed and deserializes the response with the
        /// type (T) passed.
        /// </summary>
        /// <typeparam name="T">The type used to deserialize the response.
        /// </typeparam>
        /// <param name="request">The request to execute.</param>
        /// <returns>The RestResponse from RestSharp.</returns>
        /// <exception cref="ArgumentException">Thrown if the parameter is not
        /// set.</exception>
        public async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request)
        {
            if (request == null) 
                throw new ArgumentException("The request was not set.");

            var response = await Client.ExecuteAsync<T>(request);
            LogTransaction(request, response);

            return response;
        }

        /// <summary>
        /// Used when the response JSON contains an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of object expected in each array node.
        /// </typeparam>
        /// <param name="request">The request to execute.</param>
        /// <returns>The RestResponse from RestSharp.</returns>
        /// <exception cref="ArgumentException">Thrown when the parameter is 
        /// not set.</exception>
        public async Task<RestResponse<List<T>>> ExecuteReturnsList<T>(
            RestRequest request)
        {
            if (request == null)
                throw new ArgumentException("The request was not set.");

            var response = await Client.ExecuteAsync<List<T>>(request);
            LogTransaction(request, response);

            return response;
        }

        /// <summary>
        /// This is used to log the request/response transaction to the debug 
        /// output for debugging. 
        /// Outputs success message on 200 level response.
        /// If the request is not successful or if an exception is caught by
        /// RestSharp, the request and response will be output along with any
        /// exceptions or errors in RestSharp.
        /// </summary>
        /// <param name="request">The RestSharp request.</param>
        /// <param name="response">The RestSharp response.</param>
        private void LogTransaction(RestRequest request, RestResponse response)
        {
            if (request == null)
                throw new ArgumentException("The request was not set.");
            if (response == null)
                throw new ArgumentException("The response was not set.");

            if (response.IsSuccessful && response.ErrorException == null)
            {
                Debug.WriteLine($"{Environment.NewLine}" +
                    $"{Client.BuildUri(request)} returned status " +
                    $"\"{response.StatusCode}\"" +
                    $"{Environment.NewLine}");
                return;
            }

            if (response.ErrorException != null)
            {
                Debug.WriteLine($"{Environment.NewLine}" +
                    $"RestSharp ErrorException Message: " +
                    $"{response.ErrorException.Message}" +
                    $"{Environment.NewLine}");
            }

            var logRequest = new
            {
                resouce = request.Resource,
                parameters = request.Parameters.Select(parameter =>
                    new
                    {
                        name = parameter.Name,
                        value = parameter.Value,
                        type = parameter.Type.ToString()
                    }),
                method = request.Method.ToString(),
                uri = Client.BuildUri(request)
            };
            var logResponse = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage
            };
            var message = $"{Environment.NewLine}" +
                $"The response was not Successful, instead " +
                $"{response.StatusCode} was received." +
                $"{Environment.NewLine}" +
                $"Request: {JsonConvert.SerializeObject(logRequest)}" +
                $"{Environment.NewLine}" +
                $"Response: {JsonConvert.SerializeObject(logResponse)}" +
                $"{Environment.NewLine}";
            Debug.WriteLine(message);
        }

        private void SetBearerToken()
        {
            int expireTime;

            (BearerToken, expireTime) = TokenService.GetTokenAndExpireTime(
                TokenParameters);

            // Subtract 2 minutes from the expire time for a buffer.
            expireTime = expireTime * 1000 - 120000;
            ScheduleTokenTimer(expireTime);

            if (Client != null)
            {
                Client.Authenticator = new JwtAuthenticator(BearerToken);
            }
        }

        private void ScheduleTokenTimer(int expireTime)
        {
            TokenTimer = new Timer(expireTime);
            TokenTimer.Elapsed += TimerElapsed;
            TokenTimer.Enabled = true;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            TokenTimer.Stop();
            lock (Client)
            {
                SetBearerToken();
                Debug.WriteLine("\nNew Bearer Token Retrieved.\n");
            }
        }
    }
}
