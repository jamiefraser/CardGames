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
        [FunctionName("JoinTableRequest")]
        public static async Task PublishJoinTableRequest([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "table/join")] HttpRequest req,
                                                         ILogger log,
                                                         [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages)
        {
            log.LogInformation("User requesting to join");
            var sr = new StreamReader(req.Body);
            var json = sr.ReadToEnd();
            var joinRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.RequestToJoinTableMessage>(json);
            await messages.AddAsync(
                                    new SignalRMessage
                                    {
                                        Target = "joinrequest",
                                        Arguments = new[]
                                        {
                                            joinRequest
                                        }
                                    });
        }
        [FunctionName("PublishPlayerAdmitted")]
        public static async Task<bool> PublishPlayerAdmitted(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "table/publishplayeradmitted")] HttpRequest req,
            ILogger log,
            [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            log.LogInformation("Player admitted!");
            var sr = new StreamReader(req.Body);
            var json = sr.ReadToEnd();
            try
            {
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.RequestToJoinTableMessage>(json);
                await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "playeradmitted",
                    Arguments = new[]
                    {
                        message
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
