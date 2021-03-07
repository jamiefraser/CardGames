using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared
{
    public static class Helpers
    {
        public static Entities.Card ConvertColourToSuit(this Entities.Card c)
        {
            if (c.Suit.ToLower() == "yellow") c.Suit = "diams";
            if (c.Suit.ToLower() == "green") c.Suit = "hearts";
            if (c.Suit.ToLower() == "blue") c.Suit = "clubs";
            if (c.Suit.ToLower() == "red") c.Suit = "spades";
            return c;
        }
        public static Entities.Card ConvertSuitToColour(this Entities.Card c)
        {
            if (c.Suit.ToLower() == "diams") c.Suit = "yellow";
            if (c.Suit.ToLower() == "hearts") c.Suit = "green";
            if (c.Suit.ToLower() == "clubs") c.Suit = "blue";
            if (c.Suit.ToLower() == "spades") c.Suit = "red";
            return c;
        }
        public static Entities.Player ToPlayer(this ClaimsPrincipal principal)
        {
            var player = new Entities.Player()
            {
                Hand = new List<Card>(),
                PrincipalId = principal.Claims.Where(c => c.Type == "oid").FirstOrDefault().Value,
                PrincipalName = principal.Identities.FirstOrDefault().Name,
                PrincipalIdp = principal.Identities.FirstOrDefault().AuthenticationType
            };
            return player;
        }
    }
}
