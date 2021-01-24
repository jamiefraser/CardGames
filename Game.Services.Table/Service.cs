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
                                                            ILogger log)
        {
            string messageJson = await new StreamReader(req.Body).ReadToEndAsync();
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.RequestToJoinTableMessage>(messageJson);
            var user = req.UserInfo(_config);
            var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
            var player = principal.ToPlayer();
            if(message.Table.TableOwner.PrincipalId.Equals(player.PrincipalId))
            {
                var service = Refit.RestService.For<IRTCService>(Environment.GetEnvironmentVariable("RTCBaseUrl"));
                message.Table.PlayersRequestingAccess.Remove(message.RequestingPlayer);
                await message.Table.Save();
                await service.PublishPlayerAdmitted(message);
                return new AcceptedResult();
            }
            else
            {
                return new BadRequestObjectResult(message);
            }
        }
        [FunctionName("RequestToJoinTable")]
        public async Task<IActionResult> RequestToJoinTable([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="tables/join")]HttpRequest req,
                                                            ILogger log)
        {
            try
            {
                var user = req.UserInfo(_config);
                var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
                var player = principal.ToPlayer();
                string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
                var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
                var persistedTable = await Helpers.Helpers.GetTable(table.Id.ToString());
                var service = Refit.RestService.For<IRTCService>(Environment.GetEnvironmentVariable("RTCBaseUrl"));
                table.PlayersRequestingAccess.Add(player);
                await table.Save();
                await service.PublishRequestToJoinMessage(new Entities.RequestToJoinTableMessage()
                {
                    RequestingPlayer = player,
                    Table = table,
                    TableOwnerId = table.TableOwner.PrincipalId
                });
                
                //List<string> invitedPlayerIds = table.InvitedPlayerIds != null ? new List<string>(table.InvitedPlayerIds) : new List<string>();
                //persistedTable.InvitedPlayers.Add(player);
                //invitedPlayerIds.Add(player.PrincipalId);
                //table.InvitedPlayerIds = invitedPlayerIds.ToArray();
                //await table.Save();
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
            ILogger log)
        {
            string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
            var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
            var owner = req.UserInfo(_config);
            table.TableOwner = owner;
            table.Id = Guid.NewGuid();
            table = await table.Save();
            //var service = Refit.RestService.For<IRTCService>(Environment.GetEnvironmentVariable("RTCBaseUrl"));
            //await service.PublishTableCreatedMessage(table);
            return new AcceptedResult();
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
