using Caro.Game.Enum;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task CreateSession(int _betValue, string _name)
        {
            try
            {

                var session = new GameSession
                {
                    NameTable = _name,
                    Player1 = Context.ConnectionId,
                    Player2 = string.Empty,
                    CurrentPlayer = Context.ConnectionId,
                    betValue = _betValue
                };

                foreach (var room in _sessions.Values)
                {
                    if (room.NameTable == _name)
                    {
                        await Clients.Caller.SendAsync("NameInvalid");
                        LogUtil.LogMessage("Create Session Failed cause name existed");
                        return;
                    }
                }

                if (!_sessions.TryAdd(session.SessionId, session))
                {
                    await Clients.Caller.SendAsync("ErrorMessage", "Failed to create game session");
                    LogUtil.LogMessage("Can not add session to dictionary");
                    return;
                }
                PlayerCaro player = null;
                if (_player.TryGetValue(Context.ConnectionId, out player))
                {
                    player.Status = PlayerState.InRoom;
                    _player.AddOrUpdate(Context.ConnectionId, player, (_k, _v) => _v = player);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionId);
                await Clients.Caller.SendAsync("SessionCreated", session.SessionId, session.NameTable);
                LogUtil.LogMessage("Create Sesssion :" + JsonConvert.SerializeObject(session));
            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
            }
        }
    }
}
