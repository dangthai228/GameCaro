using Caro.Game.Enum;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task JoinRoomWithName(string nametable)
        {
            try
            {
                GameSession session = null;
                foreach (var table in _sessions.Values)
                {
                    if (table.NameTable == nametable)
                    {
                        session = table;
                        break;
                    }
                }
                if (session == null)
                {
                    await Clients.Caller.SendAsync("SessionNotFound");
                    LogUtil.LogMessage("Can not found session name : "+ nametable);
                    return;
                }

                string sessionId = session.SessionId;
                if (!String.IsNullOrEmpty(session.Player2))
                {
                    await Clients.Caller.SendAsync("RoomIsFull");
                    return;
                }
                session.Player2 = Context.ConnectionId;
                _sessions.AddOrUpdate(sessionId, session, (k, _v) => _v = session);


                if (_player.TryGetValue(Context.ConnectionId, out PlayerCaro player))
                {
                    player.Status = PlayerState.InRoom;
                    _player.AddOrUpdate(Context.ConnectionId, player, (_k, _v) => _v = player);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
                await Clients.Group(session.SessionId).SendAsync("SessionJoined", session.Player1, session.Player2);
                await Clients.OthersInGroup(session.SessionId).SendAsync("SomeOneJoin");
                LogUtil.LogMessage("AcountId : "+ player.AccountId +" has join the room "+ " sessionname"+ session.NameTable);
            }
            catch(Exception ex) 
            {
                LogUtil.LogFailed(ex);
            }
        }
    }
}
