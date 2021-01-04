using Game.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Game.Services.RealTimeCommunications
{
    public static class PresenceStatusProcessor
    {
        [FunctionName("PresenceStatusProcessor")]
        public static async Task Run([QueueTrigger("presence-updates", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log, [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var statusMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.PresenceStatusMessage>(myQueueItem);
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
                catch (Exception ex)
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

            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
