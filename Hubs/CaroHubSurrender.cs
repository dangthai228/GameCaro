using Caro.Game.Enum;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task Surrender()
        {
            try
            {
                long accid = AccountClaim.getAccountID(Context);
                string sessionID = String.Empty;
                foreach (var session in _sessions.Values)
                {
                    if (session.Player1 == Context.ConnectionId || session.Player2 == Context.ConnectionId)
                    {
                        sessionID = session.SessionId;
                        break;
                    }
                }
                if (_player.ContainsKey(Context.ConnectionId))
                {
                    _player.TryGetValue(Context.ConnectionId, out var playersur);
                    if (_sessions.TryGetValue(sessionID, out var session))
                    {
                        string connectionOpposite = String.Empty;
                        if (Context.ConnectionId.Equals(session.Player1))
                        {
                            bool IsUpdate = updateAfterEndGame(session, false);
                            connectionOpposite = session.Player2;
                        }
                        else
                        {
                            bool IsUpdate = updateAfterEndGame(session, true);
                            connectionOpposite = session.Player1;
                        }

                        
                        await Clients.Caller.SendAsync("Loser");
                        await Clients.OthersInGroup(sessionID).SendAsync("Winner");


                       
                        _player.TryGetValue(connectionOpposite, out var opposite);
                        playersur.Status = PlayerState.InRoom;
                        opposite.Status = PlayerState.InRoom;

                        _player.AddOrUpdate(connectionOpposite, opposite, (k, v) => v = opposite);
                        _player.AddOrUpdate(Context.ConnectionId, opposite, (k, v) => v = playersur);


                    }

                    // renew gamesession(table, currentplayer)
                    session.CurrentPlayer = session.Player1;
                    session.Board = new string[15, 15];
                    _sessions.AddOrUpdate(sessionID, session, (k, v) => v = session);
                    LogUtil.LogMessage("AccountId : "+accid + " has surrender , game session : "+sessionID);
                }
            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
            }
            
        }
    }
}
