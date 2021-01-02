using Game.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;

namespace Game.Services.RealTimeCommunications
{
    public static class Presence
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "gameroom")] SignalRConnectionInfo connectionInfo)
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
        public static async Task UpdatePresence([HttpTrigger(AuthorizationLevel.Anonymous, "post")] Entities.PresenceStatusMessage statusMessage,
                                            [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            
            var player = statusMessage.Player;
            if (player == null) return;
                player.ETag = "*";
            player.RowKey = player.PrincipalId;
            player.PartitionKey = "Online Players";
            var table = await Helpers.Helpers.GetTableReference("onlineplayers");
            if (statusMessage.CurrentStatus.Equals(PlayerPresence.Online))
            {
                //add a row to the table
                var insertOperation = Microsoft.Azure.Cosmos.Table.TableOperation.Insert(player);
                var result = await table.ExecuteAsync(insertOperation);
            }
            else
            {
                var fetchOperation = Microsoft.Azure.Cosmos.Table.TableOperation.Retrieve<Player>(player.PartitionKey, player.RowKey);
                var retrieved = await table.ExecuteAsync(fetchOperation);
                //if (retrieved != null) player = retrieved.Result as Player;
                try
                {
                    var deleteOperation = Microsoft.Azure.Cosmos.Table.TableOperation.Delete(player);
                    var result = await table.ExecuteAsync(deleteOperation);
                }
                catch(Exception ex)
                {

                }
                //delete the row from the table
            }
            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "presence",
                Arguments = new[] { statusMessage }
            });
            return;
        }
        [FunctionName("players")]
        public static async Task<IActionResult> Players([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, ILogger log)
        {
            var players = new List<Entities.Player>();
            return new OkObjectResult(players);
        }
    }
}