using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Tsp;
using System;
using System.Linq;
using System.Security.Claims;

namespace Caro.Game.Utilts
{
    public class AccountClaim
    {
        private static long accountID;

        public static long getAccountID(HubCallerContext context)
        {
            accountID = 0;
            if (context.GetHttpContext().User != null && context.GetHttpContext().User.Identity.IsAuthenticated)
            {
                var data = context.User.FindFirst(ClaimTypes.Name).Value;
                 accountID = Convert.ToInt32(data);
            }
            return accountID;
        }
    }
}
