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
namespace Game.Services.Table
{
    public class Service
    {
        private IConfiguration _config;
        public Service(IConfiguration configuration)
        {
            _config = configuration;
        }
        [FunctionName("Start")]
        public async Task<IActionResult> StartGameAtTable([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/{tableId}/start")] HttpRequest req,
                                                    string tableId,
                                                    ILogger log,
                                                    [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> message)
        {
            var table = await Helpers.Helpers.GetTable(tableId);
            table.Started = true;
            await table.Save();
            var msg = new SignalRMessage()
            {
                Target = "tableStarted",
                Arguments = new[]
                {
                    new TableStartedMessage()
                    {
                        TableId = tableId
                    }
                }
            };
            await message.AddAsync(msg);
            return new OkResult();
        }
        [FunctionName("DealHands")]
        public async Task<IActionResult> DealHands([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables/deal/{tableId}")] HttpRequest req,
                                                    string tableId,
                                                    ILogger log,
                                                    [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> message)
        {
            var table = await Helpers.Helpers.GetTable(tableId);
            foreach (Entities.Card card in table.DealtCards)
            {
                table.Deck.Cards.Push(card);
            }
            table.DealtCards.Clear();
            table.Deck.Shuffle();
            List<Entities.Player> players = new List<Entities.Player>(table.Players.Values);
            foreach (Entities.Player p in players)
            {
                p.Hand.Clear();
            }
            foreach (KeyValuePair<int, Entities.Player> p in table.Players)
            {
                p.Value.Hand = new List<Entities.Card>();
            }
            for (int i = 0; i < table.Game.NumberOfCardsToDeal; i++)
            {
                foreach (Entities.Player p in players)
                {
                    var card = table.Deck.Cards.Pop();
                    p.Hand.Add(card);
                    table.DealtCards.Add(card);
                }
            }
            //Save and send the hands to their players
            foreach (Entities.Player p in players)
            {
                var m = new SignalRMessage()
                {
                    UserId = p.PrincipalId,
                    Arguments = new[]
                    {
                        new PlayerHandMessage()
                        {
                            Hand = p.Hand,
                            TableId = table.Id
                        }
                    },
                    Target = "handDealt"
                };
                try
                {
                    await p.Hand.Save(table, p);
                }
                catch (Exception ex)
                {
                    throw;
                }
                await message.AddAsync(m);
            }
            var c = table.Deck.Cards.Pop();
            table.DealtCards.Add(c);
            var msg = new SignalRMessage()
            {
                Arguments = new[]
                {
                    new NewDiscardedCardMessage()
                    {
                        Card = c
                    }
                },
                Target = "newCardOnDiscardPile"
            };
            await message.AddAsync(msg);
            await table.Save();
            return new OkResult();
        }
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
                
                var p = t.Players.Where(p => p.Value.PrincipalId.Equals(player.PrincipalId)).FirstOrDefault().Value;
                var cardInHand = p.Hand.Where(c => c.Suit.Equals(card.Suit) && c.Rank.Equals(card.Rank)).FirstOrDefault();
                var index = p?.Hand.IndexOf(cardInHand);
                hand.RemoveAt(index.Value);
                p.Hand.RemoveAt(index.Value);
                var msg = new SignalRMessage()
                {
                    Arguments = new[]
                    {
                        new NewDiscardedCardMessage()
                        {
                            Card = card
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
        [FunctionName("CreateTable")]
        public async Task<IActionResult> CreateTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tables")] HttpRequest req,
            ILogger log,
            [SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "gameroom")] IAsyncCollector<SignalRMessage> messages)
        {
            try
            {
                string tableJson = await new StreamReader(req.Body).ReadToEndAsync();
                var table = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.Entities.Table>(tableJson);
                var owner = req.UserInfo(_config);
                table.TableOwner = owner;
                if (table.Id.Equals(Guid.Empty))
                {
                    table.Id = Guid.NewGuid();
                }
                if (table.Game != null)
                {
                    if (table.Deck == null)
                    {
                        table.Game = table.Game;
                    }
                }
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
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("FetchTables")]
        public async Task<IActionResult> GetActiveTables([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tables")] HttpRequest reg, ILogger log)
        {
            return new OkObjectResult(await Helpers.Helpers.GetTables());
        }
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
    }
}
