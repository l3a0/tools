namespace CdsTestDriver
{
    using System;
    using System.Configuration;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Connect"].ConnectionString;

            using (var cdsClient = new CdsClient(connectionString))
            {
                for (var i = 0; i < 100; i++)
                {
                    var account = new JObject
                    {
                        { "name", $"Contoso Ltd {i}" },
                        { "telephone1", "555-5555" }
                    };

                    var entityUri = cdsClient.CreateEntityAsync("accounts", account).Result;

                    account["telephone1"] = "867-5309";
                    cdsClient.UpdateEntityAsync(entityUri, account).Wait();

                    cdsClient.DeleteEntityAsync(entityUri).Wait();

                    if (i % 10 == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }
    }
}