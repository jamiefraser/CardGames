using Game.Entities;
using Game.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Game.Services.RealTimeCommunications
{
    public static class Presence
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "gameroom", UserId ="{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo)
        {  
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static Task SendMessage([HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message, [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            string msg = string.Empty;
            if (message.GetType() != typeof(string))
            {
                FileBufferingReadStream stream = message as FileBufferingReadStream;
                if (stream.Equals(null)) throw new Exception("It would be swell if you sent a message");
                var sr = new StreamReader(stream);
                msg = sr.ReadToEnd();
            }
            else
            {
                msg = message.ToString();
            }
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "broadcast",
                    Arguments = new[] { msg }
                });
        }

        [FunctionName("UpdatePresence")]
        public static async Task<IActionResult> UpdatePresence([HttpTrigger(AuthorizationLevel.Anonymous, "post")] Entities.PresenceStatusMessage statusMessage,
                                            [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (statusMessage.Player == null) return new OkObjectResult("A request to logout was registered with no user") ;
            try
            {
                var queue = Helpers.Helpers.CreateQueueClient("presence-updates").CreateQueue();
                var message = Newtonsoft.Json.JsonConvert.SerializeObject(statusMessage);
                queue.SendMessage(message);
                return new AcceptedResult();
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("players")]
        public static async Task<IActionResult> Players([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, ILogger log)
        {
            var token = req.GetAccessToken();
            
            var tableConnection = await Helpers.Helpers.GetTableReference("onlineplayers");
            var qry = tableConnection.CreateQuery<Game.Entities.Player>();
            var players = await qry.ExecuteAsync<Game.Entities.Player>(new CancellationToken());
            return new OkObjectResult(players);
        }

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
    }
}