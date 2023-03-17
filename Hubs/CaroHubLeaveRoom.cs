using Caro.Game.Enum;
using Caro.Game.Session;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task LeaveRoom()
        {
            string sessionId = String.Empty;
            foreach (var session in _sessions.Values)
            {
                if (session.Player1 == Context.ConnectionId || session.Player2 == Context.ConnectionId)
                {
                    sessionId = session.SessionId;
                    break;
                }
            }

            if (_sessions.ContainsKey(sessionId)) 
            {
                _sessions.TryGetValue(sessionId, out var session);
                _player.TryGetValue(Context.ConnectionId, out var playerLeave);
                // chu phong roi phong 
                if (Context.ConnectionId == session.Player1)
                {
                    // update lai state 
                    playerLeave.Status = PlayerState.Online;
                    _player.AddOrUpdate(Context.ConnectionId, playerLeave, (_k, _v) => _v = playerLeave);
                    // phong co moi 1 nguoi
                    if(String.IsNullOrEmpty(session.Player2)) 
                    {
                        _sessions.TryRemove(sessionId, out _);
                    }
                    else // phong full 2 ng
                    {
                        string connectionId2 = session.Player2;
                        if(_player.ContainsKey(Context.ConnectionId))
                        {
                            session.Player1 = connectionId2;
                            session.Player2 = String.Empty;
                            session.CurrentPlayer = connectionId2;
                            _sessions.AddOrUpdate(sessionId, session, (_k, _v) => _v = session);
                        }
                        if(_player.TryGetValue(connectionId2, out var player2))
                        {
                            player2.Status = PlayerState.InRoom;
                            _player.AddOrUpdate(connectionId2,player2 , (k,v) => v= player2);
                        }

                        await Clients.OthersInGroup(sessionId).SendAsync("PartnerLeaveRoom", Context.ConnectionId);
                    }
                }
                else 
                {
                    if (_player.ContainsKey(Context.ConnectionId))
                    { 
                        if (playerLeave != null)
                        {
                            playerLeave.Status = PlayerState.Online;
                            _player.AddOrUpdate(Context.ConnectionId, playerLeave, (_k, _v) => _v = playerLeave);

                            // xoa seassion.player2 
                            session.Player2= String.Empty;
                            _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);
                        }
                    }
                    await Clients.OthersInGroup(sessionId).SendAsync("PartnerLeaveRoom", Context.ConnectionId);
                }
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

            }
        }
    }
}
