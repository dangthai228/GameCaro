using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task GetAccountInfo()
        {
            if (_player.TryGetValue(Context.ConnectionId,out var player))
            {
                await Clients.Caller.SendAsync("PlayerCaro", player);
            }
        }

        public async Task GetInventory()
        {
            long accid = AccountClaim.getAccountID(Context);
            if(_inventory.TryGetValue(accid, out var listInventory))
            {
                await Clients.Caller.SendAsync("Inventory", listInventory);
            }
        }

        public async Task GetRooms()
        {
            List<string> listsessionId = new List<string>();
            foreach(var session in _sessions.Values)
            {
                if(string.IsNullOrEmpty(session.Player2))
                {
                    listsessionId.Add(session.SessionId);
                }
            }
            await Clients.Caller.SendAsync("GetSession", listsessionId);
        }
    }
}
