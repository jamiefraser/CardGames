using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Game.Entities;
using Microsoft.Azure.Cosmos.Table;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using System.Text;

namespace Game.Services.Helpers
{
    public static class Helpers
    {
        public static async Task<Entities.Table>GetTable(string tableId)
        {
            var container = GetBlobContainerClient("tables");
            var blobClient = container.GetBlobClient(tableId);
            
            var content = blobClient.DownloadAsync().Result.Value;
            var sr = new StreamReader(content.Content);
            var tableJson = sr.ReadToEnd();
            var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Table>(tableJson);
            return table;
        }
        public static async Task<List<Entities.Game>>GetGames()
        {
            List<Entities.Game> games = new List<Entities.Game>();
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "games";

            // Create the container and return a container client object

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            foreach (var blob in (containerClient.GetBlobs()))
            {
                string name = blob.Name;
                var downloadedBlob = containerClient.GetBlobClient(name);

                using (var sr = new StreamReader(downloadedBlob.OpenRead()))
                {
                    var json = sr.ReadToEnd();
                    var game = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Game>(json);
                    games.Add(game);
                }
            }
            return games;
        }
        public static async Task<List<Entities.Table>>GetTables()
        {
            List<Entities.Table> tables = new List<Table>();
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "tables";

            // Create the container and return a container client object

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            foreach(var blob in (containerClient.GetBlobs()))
            {
                string name = blob.Name;
                var downloadedBlob = containerClient.GetBlobClient(name);
                
                using (var sr = new StreamReader(downloadedBlob.OpenRead()))
                {
                    var json = sr.ReadToEnd();
                    var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Table>(json);
                    tables.Add(table);
                }
            }
            return tables;
        }
        public static async Task<List<Card>>GetPlayerHand(Table table, Player player)
        {
            try
            {
                var name = $"{table.Id}/{player.PrincipalId}";
                BlobContainerClient containerClient = GetBlobContainerClient("hands");
                var blobClient = containerClient.GetBlobClient(name);
                using (var sr = new StreamReader(blobClient.OpenRead()))
                {
                    var json = sr.ReadToEnd();
                    var hand = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Card>>(json);
                    return hand;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task Save(this List<Card>Hand, Table table, Player player)
        {
            var name = $"{table.Id}/{player.PrincipalId}";
            BlobContainerClient containerClient = GetBlobContainerClient("hands");
            var blobClient = containerClient.GetBlobClient(name);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Hand);
            var bytes = Encoding.ASCII.GetBytes(json);
            try
            {
                var ms = new MemoryStream(bytes);
                await blobClient.UploadAsync(ms, true, new System.Threading.CancellationToken());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<Table> Save(this Table table)
        {

            var id = table.Id.Equals(null) ?  Guid.NewGuid().ToString() : table.Id.ToString();
            table.Name = string.IsNullOrEmpty(table.Name) ? $"{table.Game.Name} - {table.TableOwner.PrincipalName} - {DateTime.Now.ToString("yyyy-MM-dd")}" : table.Name;
            BlobContainerClient containerClient = GetBlobContainerClient("tables");
            var blobClient = containerClient.GetBlobClient(id);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(table);
            var bytes = Encoding.ASCII.GetBytes(json);

            try
            {
                var ms = new MemoryStream(bytes);
                await blobClient.UploadAsync(ms,true,new System.Threading.CancellationToken());
            }
            catch(Exception ex)
            {
                throw;
            }
            return table;
        }

        private static BlobContainerClient GetBlobContainerClient(string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
           

            // Create the container and return a container client object
            BlobContainerClient containerClient;
            try
            {
                containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var props = containerClient.GetProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name);
                containerClient = blobServiceClient.CreateBlobContainer(containerName);
            }

            return containerClient;
        }

        //public static async Task<Game.Entities.Table> Save(this Entities.Table tbl)
        //{

        //    var table = await GetTableReference("gametables");
        //    var insertOrMergeOperation = Microsoft.Azure.Cosmos.Table.TableOperation.InsertOrMerge(tbl);
        //    var result = await table.ExecuteAsync(insertOrMergeOperation);
        //    return result.Result as Game.Entities.Table;
        //}
        public static async Task<Microsoft.Azure.Cosmos.Table.CloudTable> GetTableReference(string tableName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            Microsoft.Azure.Cosmos.Table.CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            Microsoft.Azure.Cosmos.Table.CloudTable table = tableClient.GetTableReference(tableName);
            try
            {
                await table.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {

            }
            return table;
        }
        public static string GetAccessToken(this HttpRequest req)
        {
            var authorizationHeader = req.Headers?["Authorization"];
            string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
            if (parts.Length == 2 && parts[0].Equals("Bearer"))
                return parts[1];
            return null;
        }

        private static EasyAuthUserInfo UserInfoFromRequest(this HttpRequest req)
        {
            var stream = req.Headers["Authorization"].ToString().Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(stream) as JwtSecurityToken;
            var PrincipalId = token.Claims.Where(c => c.Type == "oid").FirstOrDefault().Value;
            var PrincipalName = token.Claims.Where(c => c.Type == "name").FirstOrDefault().Value;
            var PrincipalIdp = token.Claims.Where(c => c.Type == "idp").FirstOrDefault().Value;
            return new EasyAuthUserInfo()
            {
                PrincipalId = PrincipalId,
                PrincipalName = PrincipalName,
                PrincipalIdp = PrincipalIdp,
                ZumoHeaders = new Dictionary<string, string>()
            };
        }

        public static EasyAuthUserInfo UserInfo(this HttpRequest req, IConfiguration config)
        {
            var ret = string.IsNullOrEmpty(config["RunningLocally"]) ?
            new EasyAuthUserInfo()
            {
                PrincipalId = req.Headers.Where(h => h.Key.Equals("X-MS-CLIENT-PRINCIPAL-ID")).FirstOrDefault().Value,
                PrincipalName = req.Headers.Where(h => h.Key.Equals("X-MS-CLIENT-PRINCIPAL-NAME")).FirstOrDefault().Value,
                PrincipalIdp = req.Headers.Where(h => h.Key.Equals("X-MS-CLIENT-PRINCIPAL-IDP")).FirstOrDefault().Value,
                ZumoHeaders = new Dictionary<string, string>()
            } : req.UserInfoFromRequest();
            foreach (var h in req.Headers.Where(header => header.Key.StartsWith("X-MS")))
            {
                ret.ZumoHeaders.Add(h.Key, h.Value);
            }
            return ret;
        }
        public static Entities.Player ToPlayer(this ClaimsPrincipal principal)
        {
            var player = new Entities.Player()
            {
                Hand = new List<Card>(),
                PrincipalId = principal.Claims.Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").FirstOrDefault().Value,
                PrincipalName = !string.IsNullOrEmpty(principal.Identities.FirstOrDefault().Name) ? principal.Identities.FirstOrDefault().Name : principal.Claims.Where(c => c.Type=="name").FirstOrDefault().Value,
                PrincipalIdp = principal.Identities.FirstOrDefault().AuthenticationType
            };
            return player;
        }
        public static async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string accessToken, ILogger log)
        {
            var audience = Constants.audience;
            var b2cInstance = Constants.b2cInstance;
            var clientID = Constants.clientID;
            var tenant = Constants.tenant;
            var tenantid = Constants.tenantid;
            var aadInstance = Constants.aadInstance;
            var authority = Constants.authority;
            var validIssuers = Constants.validIssuers;
            var policyName = Constants.policyName;
            // Debugging purposes only, set this to false for production
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            ConfigurationManager<OpenIdConnectConfiguration> configManager =
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{b2cInstance}/{tenant}/{policyName}/v2.0/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever());

            OpenIdConnectConfiguration config = null;
            config = await configManager.GetConfigurationAsync();

            ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

            // Initialize the token validation parameters
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                // App Id URI and AppId of this service application are both valid audiences.
                ValidAudiences = new[] { audience, clientID },

                // Support Azure AD V1 and V2 endpoints.
                ValidIssuers = validIssuers,
                IssuerSigningKeys = config.SigningKeys
            };

            try
            {
                SecurityToken securityToken;
                var claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);
                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
            return null;
        }

        public static QueueClient CreateQueueClient(string queueName)
        {
            // Get the connection string from app settings
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            QueueClientOptions opts = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName,opts);
            return queueClient;
        }

        public static  QueueClient CreateQueue(this QueueClient queueClient)
        {
            try
            {
                // Create the queue
                queueClient.CreateIfNotExists();

                if (queueClient.Exists())
                {
                    Console.WriteLine($"Queue created: '{queueClient.Name}'");
                    
                    return queueClient;
                }
                else
                {
                    Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                    return queueClient;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\n\n");
                Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                throw;
            }
        }

    }
}
