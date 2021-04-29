using Game.Entities;
using Game.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Collections;
namespace Game.Services.Table
{
    public class Service
    {
        private IConfiguration _config;
        public Service(IConfiguration configuration)
        {
            _config = configuration;
        }

        #region Start Game
        [FunctionName("Start")]
        public async Task<IActionResult> StartGameAtTable([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/{tableId}/start")] HttpRequest req,
                                                    string tableId,
                                                    ILogger log,
                                                    [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> message)
        {
            var table = await Helpers.Helpers.GetTable(tableId);
            table.Started = true;
            table.Dealer = table.Players.First().Value;
            await table.Save();
            var msg = new SignalRMessage()
            {
                Target = "tableStarted",
                Arguments = new[]
                {
                    new TableStartedMessage()
                    {
                        TableId = tableId,
                        Dealer = table.Dealer
                    }
                }
            };
            await message.AddAsync(msg);
            return new OkResult();
        }
        #endregion

        #region Play Trick
        [FunctionName("PlayTrick")]
        public async Task<IActionResult> PlayTrick([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/playtrick/{tableId}")] HttpRequest req,
                                                    string tableId,
                                                    ILogger log,
                                                    [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> message)
        {
            return new AcceptedResult();
        }
        #endregion



        #region Publish Player Selected Card Message
        [FunctionName("PlayerSelectedCard")]
        public async Task<IActionResult>PlayerSelectedCard([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables/hand/{tableId}/selected/{cardIndex}")] HttpRequest req,
                                                     ILogger log,
                                                     [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages,
                                                     Guid tableId,
                                                     int cardIndex)
        {

            var t = await Helpers.Helpers.GetTable(tableId.ToString());
            var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
            var player = principal.ToPlayer();
            foreach(Entities.Player p in t.Players.Values)
            {
                var msg = new SignalRMessage()
                {
                    UserId = p.PrincipalId,
                    Arguments = new[]
                    {
                        new CardSelectedMessage()
                        {
                            CardIndex = cardIndex,
                            Player = player
                        }
                    },
                    Target = "playerSelectedCard"
                };
                await messages.AddAsync(msg);
            }
            return new OkResult();
        }
        #endregion

        #region Publish Player is Discarding Card Message
        [FunctionName("PlayerDiscardingCard")]
        public async Task<IActionResult>DiscardCard([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/hand/{tableId}/discard")] HttpRequest req,
                                                     ILogger log,
                                                     [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages,
                                                     string tableId)
        {
            try
            {
                var cardJson = await req.ReadAsStringAsync();
                var card = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Card>(cardJson);
                var t = await Helpers.Helpers.GetTable(tableId);
                var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
                var player = principal.ToPlayer();
                var hand = await Helpers.Helpers.GetPlayerHand(t, player);
                var thisPlayerIndex = t.Players.IndexOfValue(t.Players.Where(p => p.Value.PrincipalId.Equals(player.PrincipalId)).FirstOrDefault().Value);
                Player nextPlayer;
                if (thisPlayerIndex  +1 < t.Players.Count)
                {
                    nextPlayer = t.Players.Skip(thisPlayerIndex +1 ).Take(1).First().Value;
                }
                else
                {
                    //wrap around
                    nextPlayer = t.Players.First().Value;
                }
                var p = t.Players.Where(p => p.Value.PrincipalId.Equals(player.PrincipalId)).FirstOrDefault().Value;
                var cardInHand = p.Hand.Where(c => c.Suit.Equals(card.Suit) && c.Rank.Equals(card.Rank)).FirstOrDefault();
                var index = p?.Hand.IndexOf(cardInHand);
                hand.RemoveAt(index.Value);
                p.Hand.RemoveAt(index.Value);
                await p.Hand.Save(t, p);
                var msg = new SignalRMessage()
                {
                    Arguments = new[]
                    {
                        new NewDiscardedCardMessage()
                        {
                            Card = card,
                            NextPlayer = nextPlayer
                        }
                    },
                    Target = "newCardOnDiscardPile"
                };
                await messages.AddAsync(msg);
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new NotFoundResult();
            }
        }
        #endregion

        #region Get Hand for calling player
        [FunctionName("RetrieveHand")]
        public async Task<IActionResult> RetrieveHand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables/hand/{tableId}")] HttpRequest req,
                                                     ILogger log,
                                                     [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages,
                                                     string tableId)
        {
            try
            {
                var t = await Helpers.Helpers.GetTable(tableId);
                var principal = await Helpers.Helpers.GetClaimsPrincipalAsync(req.GetAccessToken(), log);
                var player = principal.ToPlayer();
                var hand = await Helpers.Helpers.GetPlayerHand(t, player);
                return new OkObjectResult(hand);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new NotFoundResult();
            }
        }
        #endregion

        #region Admit Player to Table
        [FunctionName("AdmitPlayer")]
        public async Task<IActionResult> AdmitPlayer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/admit")] HttpRequest req,
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
                    int key = message.Table.Players.Count == 0 ? 0 : message.Table.Players.Keys.Max() + 1;
                    message.Table.Players.Add(key, message.RequestingPlayer);
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
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        #endregion

        #region Request to Join a Table
        [FunctionName("RequestToJoinTable")]
        public async Task<IActionResult> RequestToJoinTable([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/join")] HttpRequest req,
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
            catch (Exception ex)
            {
                return new BadRequestResult();
            }
        }
        #endregion

        #region Retrieve All Available Tables
        [FunctionName("FetchTables")]
        public async Task<IActionResult> GetActiveTables([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables")] HttpRequest reg, ILogger log)
        {
            return new OkObjectResult(await Helpers.Helpers.GetTables());
        }
        #endregion

        #region Retrieve Specific Table
        [FunctionName("FetchTable")]
        public async Task<IActionResult> GetTable([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables/{id}")] HttpRequest req, string? id, ILogger log)
        {
            try
            {
                return new OkObjectResult((await Helpers.Helpers.GetTable(id)));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        #endregion
    }
}
