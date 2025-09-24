using System.Net;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk;


namespace Backend.Services
{
    public class YdbContext
    {
        /*public static async Task<TableClient> Initialize(CachedCredentialsProvider staticCredentialsProvider = null)
        {
            await DownloadFileAndWriteToTemp(Credential.JsonUrl, Credential.JsonFilePath);
            var scp = new ServiceAccountProvider(Credential.JsonFilePath);

            return await Run(Credential.Endpoint, Credential.Database, scp);
        }

        private static async Task DownloadFileAndWriteToTemp(string sourceUrl, string destinationOutputPath)
        {
            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(sourceUrl))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(destinationOutputPath, content);
            }
        }

        public static async Task<TableClient> Run(string endpoint, string database, ICredentialsProvider credentialsProvider = null)
        {
            var config = new DriverConfig(
                endpoint: endpoint,
                database: database,
                credentials: credentialsProvider
            );

            using var driver = new Driver(
                config: config
            );

            await driver.Initialize();
            using var tableClient = new TableClient(driver, new TableClientConfig());

            return tableClient;
        }*/
    }
}
