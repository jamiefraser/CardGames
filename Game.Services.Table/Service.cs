using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Game.Services.Helpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Linq;
namespace Game.Services.Table
{
    public  class Service
    {
        private IConfiguration _config;
        public Service(IConfiguration configuration)
        {
            _config = configuration;
        }
        [FunctionName("AdmitPlayer")]
        public async Task<IActionResult>AdmitPlayer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/admit")] HttpRequest req,
                                                            ILogger log,
                                                            [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages)
        {
            try
            {
                string messageJson = await new StreamReader(req.Body).ReadToEndAsync();
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.RequestToJoinTableMessage>(messageJson);
                var user = req.UserInfo(_config);
                var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
                var player = principal.ToPlayer();
                if (message.Table.TableOwner.PrincipalId.Equals(player.PrincipalId))
                {
                    var lookupPlayer = message.Table.PlayersRequestingAccess.Where(p => p.PrincipalId.Equals(message.RequestingPlayer.PrincipalId)).FirstOrDefault();
                    message.Table.PlayersRequestingAccess.Remove(lookupPlayer);
                    message.Table.Players.Add(message.RequestingPlayer);
                    await message.Table.Save();
                    var srMessage = new SignalRMessage
                    {
                        Target = "playeradmitted",
                        Arguments = new[]
                    {
                        message
                    }
                    };
                    await messages.AddAsync(srMessage);
                    return new AcceptedResult();
                }
                else
                {
                    return new BadRequestObjectResult(message);
                }
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("RequestToJoinTable")]
        public async Task<IActionResult> RequestToJoinTable([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="tables/join")]HttpRequest req,
                                                            ILogger log,
                                                            [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages)
        {
            try
            {
                var user = req.UserInfo(_config);
                var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
                var player = principal.ToPlayer();
                string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
                var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
                var persistedTable = await Helpers.Helpers.GetTable(table.Id.ToString());
                table.PlayersRequestingAccess.Add(player);
                await table.Save();
                var joinRequest = new Entities.RequestToJoinTableMessage()
                {
                    RequestingPlayer = player,
                    Table = table,
                    TableOwnerId = table.TableOwner.PrincipalId
                };
                await messages.AddAsync(
                                     new SignalRMessage
                                     {
                                         Target = "joinrequest",
                                         Arguments = new[]
                                         {
                                            joinRequest
                                         }
                                     });
                return new OkResult();
            }
            catch(Exception ex)
            {
                return new BadRequestResult();
            }
        }
        [FunctionName("CreateTable")]
        public async Task<IActionResult> CreateTable(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "tables")] HttpRequest req,
            ILogger log,
            [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName ="gameroom")]IAsyncCollector<SignalRMessage>messages)
        {
            try
            {
                string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
                var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
                var owner = req.UserInfo(_config);
                table.TableOwner = owner;
                table.Id = Guid.NewGuid();
                table = await table.Save();
                var message = new SignalRMessage
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
                };
                await messages.AddAsync(message);
                //var service = Refit.RestService.For<IRTCService>(Environment.GetEnvironmentVariable("RTCBaseUrl"));
                //await service.PublishTableCreatedMessage(table);
                return new AcceptedResult();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("FetchTables")]
        public async Task<IActionResult>GetActiveTables([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="tables")] HttpRequest reg, ILogger log)
        {
            return new OkObjectResult(await Helpers.Helpers.GetTables());
        }
        [FunctionName("FetchTable")]
        public async Task<IActionResult>GetTable([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables/{id}")]HttpRequest req, string? id, ILogger log)
        {
            try
            {
                return new OkObjectResult((await Helpers.Helpers.GetTable(id)));
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
