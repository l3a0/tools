namespace CdsTestDriver
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    internal sealed class CdsClient : ICdsClient, IDisposable
    {
        // Flag: Has Dispose already been called?
        private bool disposed;

        public CdsClient(string connectionString)
        {
            this.HttpClient = GetHttpClient(connectionString);
        }

        ~CdsClient()
        {
            this.Dispose(false);
        }

        private HttpClient HttpClient { get; }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<string> CreateEntityAsync(string entityLogicalName, JObject entity)
        {
            var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{this.HttpClient.BaseAddress}{entityLogicalName}")
            {
                Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json")
            };

            var createResponse = await this.HttpClient.SendAsync(createRequest, HttpCompletionOption.ResponseContentRead);

            if (createResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Entity '{0}' created.", entity.GetValue("name"));
                return createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault();
            }

            Console.WriteLine("Failed to create account for reason: {0}.", createResponse.ReasonPhrase);
            throw new Exception($"Failed to create account for reason: {createResponse.Content}.");
        }

        public async Task DeleteEntityAsync(string entityUri)
        {
            var deleteResponse = await this.HttpClient.DeleteAsync(entityUri);

            if (deleteResponse.IsSuccessStatusCode) //200-299 
            {
                Console.WriteLine($"Entity deleted: \n{entityUri}.");
            }
            else if (deleteResponse.StatusCode == HttpStatusCode.NotFound) //404 
            {
                //May have been deleted by another user or via cascade operation 
                Console.WriteLine($"Entity not found: {entityUri}.");
            }
            else
            {
                Console.WriteLine($"Failed to delete: {entityUri}.");
                Console.WriteLine($"Content: {deleteResponse.Content}.");
            }
        }

        public async Task UpdateEntityAsync(string entityUri, JObject entity)
        {
            var updateRequest = new HttpRequestMessage(new HttpMethod("PATCH"), entityUri)
            {
                Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json")
            };

            var updateResponse = await this.HttpClient.SendAsync(updateRequest, HttpCompletionOption.ResponseContentRead);

            if (updateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Entity updated. {entity}");
            }
            else
            {
                Console.WriteLine("Failed to update entity for reason: {0}", updateResponse.ReasonPhrase);
            }
        }

        private static HttpClient GetHttpClient(string connectionString)
        {
            var url = GetParameterValueFromConnectionString(connectionString, "Url");
            var username = GetParameterValueFromConnectionString(connectionString, "Username");
            var domain = GetParameterValueFromConnectionString(connectionString, "Domain");
            var password = GetParameterValueFromConnectionString(connectionString, "Password");
            var authType = GetParameterValueFromConnectionString(connectionString, "authtype");
            var clientId = GetParameterValueFromConnectionString(connectionString, "ClientId");
            var redirectUrl = GetParameterValueFromConnectionString(connectionString, "RedirectUrl");
            var version = GetParameterValueFromConnectionString(connectionString, "Version");
            HttpMessageHandler messageHandler;

            switch (authType)
            {
                case "Office365":
                case "IFD":

                    messageHandler = new OAuthMessageHandler(
                        url,
                        clientId,
                        redirectUrl,
                        username,
                        password,
                        new HttpClientHandler());
                    break;
                case "AD":
                    var credentials = new NetworkCredential(username, password, domain);
                    messageHandler = new HttpClientHandler { Credentials = credentials };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(authType), "Valid authType values are 'Office365', 'IFD', or 'AD'.");
            }

            var httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri($"{url}/api/data/{version}/"),

                Timeout = new TimeSpan(0, 2, 0) //2 minutes
            };

            return httpClient;
        }

        private static string GetParameterValueFromConnectionString(string connectionString, string parameter)
        {
            try
            {
                return connectionString.Split(';').FirstOrDefault(s => s.Trim().StartsWith(parameter))?.Split('=')[1];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        // Protected implementation of Dispose pattern.
        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
                this.HttpClient.Dispose();
            }

            // Free any unmanaged objects here.
            //
            this.disposed = true;
        }
    }
}