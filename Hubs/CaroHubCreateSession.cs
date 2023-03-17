using Caro.Game.Enum;
using Caro.Game.Models;
using Caro.Game.Session;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task CreateSession(int _betValue, string _name)
        {
            var session = new GameSession
            {
                NameTable= _name,
                Player1 = Context.ConnectionId,
                Player2 = string.Empty,
                CurrentPlayer = Context.ConnectionId,
                betValue = _betValue
            };

            if (!_sessions.TryAdd(session.SessionId, session))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Failed to create game session");
            }
            PlayerCaro player = null;
            if (_player.TryGetValue(Context.ConnectionId, out player))
            {
                player.Status = PlayerState.InRoom;
                _player.AddOrUpdate(Context.ConnectionId, player, (_k, _v) => _v = player);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionId);
            await Clients.Caller.SendAsync("SessionCreated", session.SessionId , session.NameTable);
        }
    }
}
