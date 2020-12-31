using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Game.Entities;
using Azure.Storage.Blobs;
using System.Text;
using System.Collections.Generic;
using Azure.Storage.Blobs.Models;
using Azure;
using Microsoft.Azure.Cosmos.Table;
using System.Threading;
using System.Linq;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Game.Services;
using Game.Services.Helpers;
namespace Game.Play
{
    public static class Service
    {
        private async static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this TableQuery<TElement> tableQuery, CancellationToken ct)
        {
            var nextQuery = tableQuery;
            var continuationToken = default(TableContinuationToken);
            var results = new List<TElement>();

            do
            {
                //Execute the next query segment async.
                var queryResult = await nextQuery.ExecuteSegmentedAsync(continuationToken, ct);

                //Set exact results list capacity with result count.
                results.Capacity += queryResult.Results.Count;

                //Add segment results to results list.
                results.AddRange(queryResult.Results);

                continuationToken = queryResult.ContinuationToken;

                //Continuation token is not null, more records to load.
                if (continuationToken != null && tableQuery.TakeCount.HasValue)
                {
                    //Query has a take count, calculate the remaining number of items to load.
                    var itemsToLoad = tableQuery.TakeCount.Value - results.Count;

                    //If more items to load, update query take count, or else set next query to null.
                    nextQuery = itemsToLoad > 0
                        ? tableQuery.Take<TElement>(itemsToLoad).AsTableQuery()
                        : null;
                }

            } while (continuationToken != null && nextQuery != null && !ct.IsCancellationRequested);

            return results;
        }
        [FunctionName("ListGames")]
        public static async Task<IActionResult>ListGames([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game")] HttpRequest req,
            ILogger log)
        {
            var token = req.GetAccessToken();
            var user = await Helpers.GetClaimsPrincipalAsync(token, log);
            if(user==null)
            {
                return new UnauthorizedResult();
            }
            var tableConnection = await Helpers.GetTableReference("games");
            var qry = tableConnection.CreateQuery<Game.Entities.Game>();
            //new Microsoft.Azure.Cosmos.Table.TableQuery<Game.Entities.Game>();
            qry.FilterString = "";
            try
            {
                var games = await qry.ExecuteAsync<Game.Entities.Game>(new CancellationToken());
                if(games==null || games.Count()==0)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(games);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("CreateGame")]
        public static async Task<IActionResult> CreateGame(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "game")] HttpRequest req,
            ILogger log)
        {
            var sr = new StreamReader(req.Body);
            var gameJson = sr.ReadToEnd();
            var game = JsonConvert.DeserializeObject<Game.Entities.Game>(gameJson);
            game.PartitionKey = "Card Games";
            if (string.IsNullOrEmpty(game.RowKey))
            {
                return new BadRequestObjectResult(new
                {
                    parameter = game,
                    message = "You must provide a game name (RowKey) before saving"
                });
            }
            await game.Save();
            return new AcceptedResult("",game);
        }
        [FunctionName("UpdateGame")]
        public static async Task<IActionResult> UpdateGame(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "game")] HttpRequest req,
            ILogger log)
        {
            var sr = new StreamReader(req.Body);
            var gameJson = sr.ReadToEnd();
            var game = JsonConvert.DeserializeObject<Game.Entities.Game>(gameJson);
            if (string.IsNullOrEmpty(game.RowKey))
            {
                return new BadRequestObjectResult(new
                {
                    parameter = game,
                    message = "You must provide a game name (RowKey) before saving"
                });
            }
            var tableRef = await Helpers.GetTableReference("games");
            var retrieveGameOperation = Microsoft.Azure.Cosmos.Table.TableOperation.Retrieve<Game.Entities.Game>(game.PartitionKey, game.RowKey);
            var existingGame = await tableRef.ExecuteAsync(retrieveGameOperation);
            if(existingGame.Result==null)
            {
                return new NotFoundObjectResult(game);
            }
            await game.Save();
            return new AcceptedResult("",game);
        }
        private static async Task<Game.Entities.Game> Save(this Entities.Game game)
        {
            game.Timestamp = DateTime.Now;
            var table = await Helpers.GetTableReference("games");
            var insertOrMergeOperation = Microsoft.Azure.Cosmos.Table.TableOperation.InsertOrMerge(game);
            var result = await table.ExecuteAsync(insertOrMergeOperation);
            return result.Result as Game.Entities.Game;
        }


        private static BlobContainerClient GetBlobContainerClient()
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "games";

            // Create the container and return a container client object

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            return containerClient;
        }
    }
}
