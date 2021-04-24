using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http.Json;
namespace Game.Client.Client
{
    public static class Helpers
    {
        public static Player ToPlayer(this EasyAuthUserInfo user)
        {
            try
            {
                return new Player()
                {
                    PrincipalId = user.PrincipalId,
                    PrincipalIdp = user.PrincipalIdp,
                    PrincipalName = user.PrincipalName
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static EasyAuthUserInfo ToUser(this ClaimsPrincipal identity)
        {
            try
            {
                var ret = new EasyAuthUserInfo()
                {
                    PrincipalId = identity.Claims.Where(c => c.Type == "oid").FirstOrDefault().Value,
                    PrincipalName = identity.Claims.Where(c => c.Type == "name").FirstOrDefault().Value,
                    PrincipalIdp = identity.Claims.Where(c => c.Type == "idp").FirstOrDefault().Value
                };
                return ret;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public static async Task  UpdateStatus(System.Security.Claims.ClaimsPrincipal claimsPrincipal, IHttpClientFactory factory, bool online)
        {

            var client = factory.CreateClient("presenceAPI");
            var presenceMessage = new Entities.PresenceStatusMessage()
            {
                CurrentStatus = online ? Entities.PlayerPresence.Online : Entities.PlayerPresence.Offline,
                InAGame = false,
                Player = claimsPrincipal.ToUser().ToPlayer()
            };
            await client.PostAsJsonAsync("api/updatepresence", presenceMessage);
        }
        public static async Task SendBroadcastMessage(string message, IHttpClientFactory factory)
        {
            var client = factory.CreateClient("presenceAPI");
            await client.PostAsJsonAsync("api/messages", message);
        }
    }
}
