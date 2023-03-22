using Caro.Game.DBAccess.Model;
using Caro.Game.Models;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task BuyItem(int iditem , int daysbrr)
        {
            long accid = AccountClaim.getAccountID(Context);
            try
            {
                if(!_player.TryGetValue(Context.ConnectionId, out var playerbuy))
                {
                    LogUtil.LogMessage("Cannot get info player account id = "+ accid);
                    return;
                }
                 bool res = gameDAO.BuyItem(accid, iditem, daysbrr);
                if (res) 
                {
                    // update list inventory 
                    List<Inventory> newlist = gameDAO.getInventory(accid);
                    _inventory.AddOrUpdate(accid, newlist, (k, v) => v = newlist);
                    string action = String.Empty;
                    if(daysbrr == 0)
                    {
                        LogUtil.LogMessage("AccountId = " + accid + " has bought itemid = " + iditem);
                    }
                    else
                    {
                        LogUtil.LogMessage("AccountId = " + accid + " has borrowed itemid = " + iditem+" in "+daysbrr+" days");
                    }
                    await Clients.Caller.SendAsync("BuySuccess", iditem);
                }
                else
                {
                    await Clients.Caller.SendAsync("BuyFailed", iditem);
                    LogUtil.LogMessage("Action failed when AccountId = "+accid+ " buying iditem = " +iditem);
                }
                
            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
            }
        }
    }
}
