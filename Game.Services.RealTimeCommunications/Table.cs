using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Game.Services.RealTimeCommunications
{
    public static class Table
    {
        [FunctionName("Table")]
        public static async Task<bool> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "table/created")] HttpRequest req,
            ILogger log,
            [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            log.LogInformation("New Table created!");
            var sr = new StreamReader(req.Body);
            var json = sr.ReadToEnd();
            try
            {
                var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Table>(json);
                table.InvitedPlayerIds = new System.Collections.Generic.List<string>().ToArray();
                log.LogInformation($"Notifying players that table {table.Name} is available to play");
                await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newtable",
                    Arguments = new[]
                    {
                        new Entities.TableCreationOrDeletionMessage()
                        {
                            Action = Entities.TableAction.Added,
                            Table = table
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
