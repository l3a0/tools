namespace CdsTestDriver
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    ///Custom HTTP message handler that uses OAuth authentication thru ADAL.
    /// </summary>
    internal class OAuthMessageHandler : DelegatingHandler
    {
        private readonly AuthenticationHeaderValue authHeader;

        public OAuthMessageHandler(
            string serviceUrl,
            string clientId,
            string redirectUrl,
            string username,
            string password,
            HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            var ap = AuthenticationParameters.CreateFromResourceUrlAsync(
                new Uri(serviceUrl + "/api/data/")).Result;
            var authContext = new AuthenticationContext(ap.Authority, false);
            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.

            var cred = new UserCredential(username, password);
            var authResult = authContext.AcquireToken(serviceUrl, clientId, cred);

            this.authHeader = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = this.authHeader;
            return base.SendAsync(request, cancellationToken);
        }
    }
}