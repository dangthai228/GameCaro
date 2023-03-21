using Caro.Game.Enum;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task KickPlayer()
        {
            try
            {
                string connectionId = Context.ConnectionId;
                GameSession table = null;
                foreach (var session in _sessions.Values)
                {
                    if (session.Player1 == connectionId && session.Player2 != string.Empty)
                    {
                        table = session;
                        break;
                    }
                }

                if (table != null)
                {
                    string connection_kicked = table.Player2;
                    await Clients.Caller.SendAsync("KickPartner");
                    await Clients.Client(table.Player2).SendAsync("Kicked");

                    // xoa khoi group
                    await Groups.RemoveFromGroupAsync(table.Player2, table.SessionId);
                    table.Player2 = string.Empty;
                    // update lai gamesession
                    _sessions.AddOrUpdate(table.SessionId, table, (k, v) => v = table);

                    //update state player 2
                    if (_player.TryGetValue(connection_kicked, out var playerkicked))
                    {
                        playerkicked.Status = PlayerState.Online;
                        _player.AddOrUpdate(connection_kicked, playerkicked, (k, v) => v = playerkicked);
                    }

                }
                else
                {
                    await Clients.Caller.SendAsync("ErrorKick", "You are not room master");
                }
            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
            }
           
        }
    }
}
