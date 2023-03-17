using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task MakeMove( int row, int col)
        {
            string sessionId = String.Empty;
            foreach (var Session in _sessions.Values)
            {
                if (Session.Player1 == Context.ConnectionId || Session.Player2 == Context.ConnectionId)
                {
                    sessionId = Session.SessionId;
                    break;
                }
            }
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                throw new Exception($"Game session {sessionId} not found");
            }

            if (Context.ConnectionId != session.CurrentPlayer)
            {
                throw new Exception("Not your turn");
            }

            var player = session.Player1 == Context.ConnectionId ? "X" : "O";

            if (session.Board[row, col] != null)
            {
                throw new Exception("Cell is already occupied");
            }

            session.Board[row, col] = player;

            await Clients.OthersInGroup(session.SessionId).SendAsync("EnemyMove", row, col, player);
            await Clients.Caller.SendAsync("YouMove", row, col, player);
            if (CheckWin(session.Board, player))
            {
                await Clients.Caller.SendAsync("Winner");
                await Clients.OthersInGroup(session.SessionId).SendAsync("Loser");
                // renew gamesession(table, currentplayer)
                session.CurrentPlayer = session.Player1;
                session.Board =  new string[15, 15];
                _sessions.AddOrUpdate(sessionId, session, (k,v) => v= session);

                // update db
                bool IsUpdate = updateAfterEndGame(session , true);

                //await Clients.Group(session.SessionId).SendAsync("UpdateAfterEnd", IsUpdate);
               
                return;
            }

            if (CheckDraw(session.Board))
            {

                // renew gamesession(table, currentplayer)
                session.CurrentPlayer = session.Player1;
                session.Board = new string[15, 15];
                _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);

                await Clients.Group(session.SessionId).SendAsync("GameDraw");
                bool IsUpdate = updateDraw(session);
               

                return;
            }

            session.CurrentPlayer = session.CurrentPlayer == session.Player1 ? session.Player2 : session.Player1;
            await Clients.Client(session.CurrentPlayer).SendAsync("YourTurn");
        }
    }
}
