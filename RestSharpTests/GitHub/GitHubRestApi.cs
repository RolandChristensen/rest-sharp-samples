using Newtonsoft.Json;
using RestSharp;
using RestSharpLibrary;
using RestSharpTests.GitHub.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpTests.GitHub
{
    /// <summary>
    /// 
    /// </summary>
    internal class GitHubRestApi : MultiThreadedBase
    {
        internal const string GitHubBase = "https://api.github.com";

        public GitHubRestApi() : base(new Uri(GitHubBase),
            SetUpFixture.Configuration["GitHub:PersonalAccessToken"])
        {

        }

        public async Task<string> GetZen()
        {
            var request = new RestRequest("zen");

            var response = await ExecuteAsync(request);
            ThrowExceptionOnErrors(response);

            return response.Content;
        }

        public async Task<UserProfile> GetUserProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("The username was not set.");

            var request = new RestRequest($"users/{username}");

            var response = await ExecuteAsync<UserProfile>(request);
            ThrowExceptionOnErrors(response);

            return response.Data;
        }

        public async Task<UserProfile> GetUserProfile2(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("The username was not set.");

            var request = new RestRequest($"users/{username}");

            var response = await ExecuteAsync(request);
            ThrowExceptionOnErrors(response);

            var content = JsonConvert.DeserializeObject<UserProfile>(
                response.Content);

            return content;
        }

        public async Task<string> GetUserRepos(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("The username was not set.");

            var request = new RestRequest($"users/{username}/repos");

            var response = await ExecuteAsync(request);
            ThrowExceptionOnErrors(response);

            return response.Content;
        }

        /// <summary>
        /// The GitHub documentation seems to lie about this. To get your own
        /// repos, including private repos use the URL {base}/user/repos with
        /// a personal access token that gives you repos permissions.
        /// </summary>
        /// <returns>A list of both private and public repos for the 
        /// user/organization that created the personal access token used to 
        /// authenticate.
        /// </returns>
        public async Task<List<ReposResponse>> GetYourRepos()
        {
            var request = new RestRequest($"user/repos");

            var response = await ExecuteReturnsList<ReposResponse>(request);
            ThrowExceptionOnErrors(response);

            return response.Data;
        }

        /// <summary>
        /// This will throw an exception when the response is not successful,
        /// if RestSharp had an error in its ErrorException, or both.
        /// 
        /// Add this to all requests you want to throw exceptions on failure.
        /// Alternatively, check the response for IsSuccessful and 
        /// ErrorException then return an appropriate response based on that.
        /// </summary>
        /// <param name="response">The RestResponse from a request.</param>
        /// <exception cref="ArgumentException">Thrown if the parameter is not 
        /// set.</exception>
        /// <exception cref="ApplicationException">Thrown if the RestResponse
        /// was not a 200 level response or if the ErrorException was set.
        /// </exception>
        private void ThrowExceptionOnErrors(RestResponse response)
        {
            if (response == null)
                throw new ArgumentException("The response was not set.");

            if (response.IsSuccessful && response.ErrorException == null)
                return;

            var message = "";
            if (response.ErrorException != null)
            {
                message = $"RestSharp ErrorException Message: " +
                    $"{response.ErrorException.Message}" +
                    $"{Environment.NewLine}";
            }

            var logResponse = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage
            };
            message += $"The response was not Successful!" +
                $"{Environment.NewLine}" +
                $"Response: {JsonConvert.SerializeObject(logResponse)}";
            throw new ApplicationException(message);
        }
    }
}
