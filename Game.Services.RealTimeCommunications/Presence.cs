using Game.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public static Task SendMessage([HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,[SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "NewMessage",
                    Arguments = new[] { message }
                });
        }

        [FunctionName("UpdatePresence")]
        public static Task UpdatePresence([HttpTrigger(AuthorizationLevel.Anonymous, "post")] Entities.PresenceStatusMessage statusMessage,
                                            [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var player = statusMessage.Player;
            if(statusMessage.CurrentStatus.Equals(PlayerPresence.Online))
            {
                //add a row to the table
                player.RowKey= player.PrincipalId;
                player.PartitionKey = "Online Players";
                var table = await Helpers.Helpers.GetTableReference("onlineplayers");
                var insertOperation = Microsoft.Azure.Cosmos.Table.
            }
            else
            {
                //delete the row from the table
            }
            return signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "presence",
                Arguments = new[] { statusMessage }
            });
        }
        [FunctionName("players")]
        public static async Task<IActionResult> Players([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, ILogger log)
        {
            var players = new List<Entities.Player>();
            return new OkObjectResult(players);
        }
    }
}