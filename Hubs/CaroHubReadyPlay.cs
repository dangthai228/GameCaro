using Caro.Game.Enum;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    { 
        public async Task ReadyPlay()
        {
            string sessionId = String.Empty;
            foreach (var session in _sessions.Values)
            {
                if(session.Player1 == Context.ConnectionId || session.Player2 == Context.ConnectionId)
                {
                    sessionId = session.SessionId;
                    break;
                }
            }
            string oopponent = String.Empty;
            if (_sessions.ContainsKey(sessionId)) 
            {
                if(_sessions.TryGetValue(sessionId, out var session))
                {
                    // Kiem tra xem doi thu ready chua, neu roi thi ca 2 state sng ingame
                    if (session.Player1 == Context.ConnectionId)
                    {
                        oopponent = session.Player2;
                    }
                    else
                    {
                        oopponent = session.Player1;
                    }
                    
                    if(_player.TryGetValue(oopponent, out var otherplayer ))
                    {
                        _player.TryGetValue(Context.ConnectionId, out var player);
                        if (otherplayer.Status == PlayerState.Ready_To_Play)
                        {
                            otherplayer.Status = PlayerState.InGame;
                            player.Status = PlayerState.InGame;

                            _player.AddOrUpdate(oopponent, otherplayer, (k, v) => v = otherplayer);
                            _player.AddOrUpdate(Context.ConnectionId, player, (k, v) => v = player);

                            await Clients.Group(sessionId).SendAsync("AllreadyToPlay");
                            await Clients.Client(session.CurrentPlayer).SendAsync("PlayFirst");
                        }
                        else
                        {
                            player.Status = PlayerState.Ready_To_Play;
                            _player.AddOrUpdate(Context.ConnectionId, player, (k, v) => v = player);
                            await Clients.Group(sessionId).SendAsync("ReadyToPlay",Context.ConnectionId);
                        }
                    }

                }
                
            }
        }
    }
}
