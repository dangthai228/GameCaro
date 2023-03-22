using Caro.Game.DBAccess.DAOImpl;
using Caro.Game.DBAccess.Model;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task GetAccountInfo()
        {
            try
            {
                if (_player.TryGetValue(Context.ConnectionId, out var player))
                {
                    await Clients.Caller.SendAsync("PlayerCaro", player);
                }
            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
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
                    listsessionId.Add(session.NameTable);
                }
            }
            await Clients.Caller.SendAsync("GetSession", listsessionId);
        }

        public async Task GetShop()
        {
            try
            {
                long accid = AccountClaim.getAccountID(Context);
                List<ShopItem> listshop = gameDAO.GetShop();
                await Clients.Caller.SendAsync("GetShop", listshop);
            }
            catch( Exception ex)
            {
                LogUtil.LogFailed(ex);  
            }
            
        }
    }
}
