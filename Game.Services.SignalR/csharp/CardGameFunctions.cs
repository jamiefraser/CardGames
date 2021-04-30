using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Game.Services.Helpers;
using System.Security.Claims;
using System.Linq;
using Microsoft.IdentityModel.Logging;
using Game.Entities;
using Microsoft.Extensions.Configuration;

namespace FunctionApp
{
    public class CardGameFunctions : ServerlessHub
    {
        private const string NewMessageTarget = "newMessage";
        private const string NewConnectionTarget = "newConnection";
        private IConfiguration config;
        public CardGameFunctions(IConfiguration _config)
        {
            config = _config;
        }
        [FunctionName("AdmitFromSignalR")]
        public static async Task AdmitFromSignalR([SignalRTrigger] InvocationContext invocationContext, string message, [SignalR(HubName = "gameroom")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var msg = new SignalRMessage()
            {
                Arguments = new[]
                {
                    $"Somebody said {message}"
                },
                Target = "AdmitFromSignalRResponse"
            };
            await signalRMessages.AddAsync(msg);
        }


        #region Negotiate
        [FunctionName("negotiate")]
        public async Task<SignalRConnectionInfo> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
    [SignalRConnectionInfo(HubName = "gameroom", UserId = "{headers.x-ms-signalr-user-id}")] SignalRConnectionInfo connectionInfo)
        {
            //return connectionInfo;
            var claims = GetClaims(req.Headers["Authorization"]);
            foreach (var c in claims)
            {
                if (c.Type == "nbf" || c.Type == "exp")
                {
                    System.Diagnostics.Debug.WriteLine($"{c.Type}: {c.Value}");
                }
            }
            var filteredClaims = from Claim c in claims
                                     //where c.Type != "aud"
                                     //&& c.Type != "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                                 select c;
            var filteredClaimsList = filteredClaims.ToList();
            try
            {
                return await NegotiateAsync(new Microsoft.Azure.SignalR.Management.NegotiationOptions()
                {
                    Claims = filteredClaimsList,
                    UserId = req.Headers["x-ms-signalr-user-id"]
                });
                System.Diagnostics.Debug.WriteLine(req.Headers["x-ms-signalr-user-id"]);
                //return Negotiate(req.Headers["X-Ms-Signalr-User-Id"], GetClaims(req.Headers["Authorization"]));
            }
            catch (Exception ex)
            {
                throw;
            }
        } 
        #endregion

        #region SignalR Create New Table
        [FunctionName(nameof(CreateTable))]
        public async Task CreateTable([SignalRTrigger] InvocationContext invocation, 
                                      Game.Entities.RequestCreateNewTableMessage message,
                                      ILogger log)
        {
            //var message = (Game.Entities.RequestCreateNewTableMessage)_message;
            var table = message.Table;
            var owner = message.Owner;
            try
            {
                table.TableOwner = owner.ToEasyAuthUserInfo();
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

                var msg = new SignalRMessage
                {
                    Target = "newtable",
                    Arguments = new[]
                    {
                        new Game.Entities.TableCreationOrDeletionMessage()
                        {
                            Action = Game.Entities.TableAction.Added,
                            Table = table
                        }
                    }
                };
                await Clients.All.SendAsync("newtable", msg.Arguments.First());
                ///await messages.AddAsync(msg);
                //var service = Refit.RestService.For<IRTCService>(Environment.GetEnvironmentVariable("RTCBaseUrl"));
                //await service.PublishTableCreatedMessage(table);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
        }
        #endregion

        #region Request to Join a Table
        [FunctionName("RequestToJoinTable")]
        public async Task RequestToJoinTable([SignalRTrigger]InvocationContext invocation,
                                                            RequestToJoinTableMessage message,
                                                            ILogger log)        
        {
            try
            {
                var player = message.RequestingPlayer;
                var table = message.Table;
                var persistedTable = await Helpers.GetTable(table.Id.ToString());
                table.PlayersRequestingAccess.Add(player);
                await table.Save();
                var joinRequest = new RequestToJoinTableMessage()
                {
                    RequestingPlayer = player,
                    Table = table,
                    TableOwnerId = table.TableOwner.PrincipalId
                };
                await Clients.All.SendAsync("joinrequest", joinRequest);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region RegisterPlayerStatus
        [FunctionName(nameof(RegisterPlayerStatus))]
        public async Task RegisterPlayerStatus([SignalRTrigger] InvocationContext invocation,
                                                Game.Entities.PresenceStatusMessage message,
                                                ILogger log)
        {
            //logger.LogInformation("Starting to process player status update");
            await Clients.All.SendAsync("presence", new PresenceStatusMessage()
            {
                CurrentStatus = message.CurrentStatus,
                InAGame = false,
                Player = message.Player
            }
            );
            if (message.Player == null) throw new Exception("A request to logout was registered with no user");
            try
            {
                //var queue = Game.Services.Helpers.Helpers.CreateQueueClient("presence-updates").CreateQueue();
                //var msg = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                //queue.SendMessage(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion

        #region Events
        [FunctionName(nameof(OnDisconnected))]
        public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
        {
        }

        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            await Clients.All.SendAsync(NewConnectionTarget, new NewConnection(invocationContext.ConnectionId, auth));
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
        }

        #endregion

        #region Chat
        [FunctionAuthorize]
        [FunctionName(nameof(Broadcast))]
        public async Task Broadcast([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            await Clients.All.SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
            logger.LogInformation($"{invocationContext.ConnectionId} broadcast {message}");
        }

        [FunctionName(nameof(SendToGroup))]
        public async Task SendToGroup([SignalRTrigger] InvocationContext invocationContext, string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
        }

        [FunctionName(nameof(SendToUser))]
        public async Task SendToUser([SignalRTrigger] InvocationContext invocationContext, string userName, string message)
        {
            await Clients.User(userName).SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
        }

        [FunctionName(nameof(SendToConnection))]
        public async Task SendToConnection([SignalRTrigger] InvocationContext invocationContext, string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
        }

        [FunctionName(nameof(JoinGroup))]
        public async Task JoinGroup([SignalRTrigger] InvocationContext invocationContext, string connectionId, string groupName)
        {
            await Groups.AddToGroupAsync(connectionId, groupName);
        }

        [FunctionName(nameof(LeaveGroup))]
        public async Task LeaveGroup([SignalRTrigger] InvocationContext invocationContext, string connectionId, string groupName)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        [FunctionName(nameof(JoinUserToGroup))]
        public async Task JoinUserToGroup([SignalRTrigger] InvocationContext invocationContext, string userName, string groupName)
        {
            await UserGroups.AddToGroupAsync(userName, groupName);
        }

        [FunctionName(nameof(LeaveUserFromGroup))]
        public async Task LeaveUserFromGroup([SignalRTrigger] InvocationContext invocationContext, string userName, string groupName)
        {
            await UserGroups.RemoveFromGroupAsync(userName, groupName);
        }

        #endregion

        #region Other Classes (from sample)
        private class NewConnection
        {
            public string ConnectionId { get; }

            public string Authentication { get; }

            public NewConnection(string connectionId, string authentication)
            {
                ConnectionId = connectionId;
                Authentication = authentication;
            }
        }

        private class NewMessage
        {
            public string ConnectionId { get; }
            public string Sender { get; }
            public string Text { get; }

            public NewMessage(InvocationContext invocationContext, string message)
            {
                Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
                ConnectionId = invocationContext.ConnectionId;
                Text = message;
            }
        }

        [FunctionName("index")]
        public IActionResult GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            Console.WriteLine(path);
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }
        #endregion
    }
}
