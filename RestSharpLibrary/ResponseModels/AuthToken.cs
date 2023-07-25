namespace RestSharpLibrary.ResponseModels
{
    /// <summary>
    /// The response from the Authentication Service used to get a token.
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// The token used to authenticate each request.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The scope the token allows.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// The type of token (i.e. Bearer).
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// The ticks left before the token expires.
        /// </summary>
        public int ExpiresIn { get; set; }
    }
}
