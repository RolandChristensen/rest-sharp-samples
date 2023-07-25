using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace RestSharpLibrary
{
    /// <summary>
    /// This class is designed for synchronous execution. If the application is
    /// guaranteed to only send a single request at a time, this is a good 
    /// option. Multi-threaded asynchronous applications are a lot more useful,
    /// but this is a starting point for more advanced scenarios.
    /// 
    /// After executing a
    /// request the properties of the class are set based on the response. 
    /// RestSharp ErrorException, IsSuccessful, StatusCode, and 
    /// StatusDescription are set and can be read from the instantiated object 
    /// to determine how to handle failures.
    ///
    /// If you use the client to run requests asynchronously in parallel, you 
    /// cannot rely on those properties and will instead need to use a 
    /// different method.
    /// </summary>
    public class SingleThreadedBase
    {
        /// <summary>
        /// The RestSharp client.
        /// </summary>
        private RestClient Client { get; }

        /// <summary>
        /// Holds the ErrorException property of the response.
        /// </summary>
        public Exception RestSharpErrorException { get; set; }

        /// <summary>
        /// Holds the IsSuccessful property of the response.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Holds the status code of the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Holds the status description of the response.
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Use this for NTLM authentication, such as Azure DevOps 5.0.
        /// </summary>
        /// <param name="baseUri">The base URL of the service.</param>
        /// <param name="user">The username used to authorize requests.</param>
        /// <param name="pass">The password used to authorize requests.</param>
        /// <exception cref="ArgumentException">Thrown when parameters are not 
        /// set.</exception>
        public SingleThreadedBase(Uri baseUri, string user, string pass)
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
        /// Used to run a request using a template to define the response JSON.
        ///
        /// This throws an exception on failure.
        /// Check the properties to determine the reason why it failed.
        /// </summary>
        /// <typeparam name="T">The class to deserialize into JSON.</typeparam>
        /// <param name="request">The RestRequest to execute.</param>
        /// <returns>The response data.</returns>
        public T Execute<T>(RestRequest request) where T : new()
        {
            var response = Client.Execute<T>(request);

            RestSharpErrorException = response.ErrorException;
            IsSuccessful = response.IsSuccessful;
            StatusCode = response.StatusCode;
            StatusDescription = response.StatusDescription;

            LogTransaction(request, response);

            return response.Data;
        }

        /// <summary>
        /// Executes the request passed, but does not deserialize the response.
        /// 
        /// This can be useful for getting a response from the service before
        /// knowing what the response looks like. You can then use the raw 
        /// content to create a class to deserialize the response.
        /// </summary>
        /// <param name="request">The RestRequest to send.</param>
        /// <returns>The content of the response as a string.</returns>
        /// <exception cref="ArgumentException">Thrown if the request parameter
        /// is not set.</exception>
        public string ExecuteReturnsContent(RestRequest request)
        {
            if (request == null)
                throw new ArgumentException("The request was not set.");

            var response = Client.Execute(request);
            LogTransaction(request, response);

            return response.Content;
        }

        /// <summary>
        /// Used when the response JSON contains an array of objects.
        /// </summary>
        /// <typeparam name="T">The type of response expected in the list.
        /// </typeparam>
        /// <param name="request">The RestRequest to send.</param>
        /// <returns>A list of objects.</returns>
        /// <exception cref="ArgumentException">Thrown if the request parameter
        /// is not set.</exception>
        public List<T> ExecuteReturnsList<T>(RestRequest request)
        {
            if (request == null)
                throw new ArgumentException("The request was not set.");

            var response = Client.Execute<List<T>>(request);
            LogTransaction(request, response);

            return response.Data;
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

            if (response.IsSuccessful)
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
    }
}
