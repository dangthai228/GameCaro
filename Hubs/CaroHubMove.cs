using Caro.Game.Utilts;
using Microsoft.AspNetCore.SignalR;
using NLog.Fluent;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task MakeMove( int row, int col)
        {
            try
            {
                string sessionId = String.Empty;
                long accid = AccountClaim.getAccountID(Context);
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
                    //throw new Exception($"Game session {sessionId} not found");
                    LogUtil.LogMessage("AccountID : " + accid + " can not get session ");
                    return;
                }

                if (Context.ConnectionId != session.CurrentPlayer)
                {
                    //throw new Exception("Not your turn");
                    return;
                }
                if (row >= 15 && col >= 15)
                {
                    await Clients.Caller.SendAsync("msgingame", " value invalid ");
                    return;
                }
                var player = session.Player1 == Context.ConnectionId ? "X" : "O";

                if (session.Board[row, col] != null)
                {
                    //throw new Exception("Cell is already occupied");
                    await Clients.Caller.SendAsync("msgingame", " Cell is already occupied ");
                    return;
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
                    session.Board = new string[15, 15];
                    _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);

                    // update db
                    bool IsUpdate = updateAfterEndGame(session, true);

                    //await Clients.Group(session.SessionId).SendAsync("UpdateAfterEnd", IsUpdate);
                    LogUtil.LogMessage("Game session : " + session.SessionId + "Is End , Winner is " + accid);
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
                    LogUtil.LogMessage("Game session : " + session.SessionId + "Is Draw");

                    return;
                }

                session.CurrentPlayer = session.CurrentPlayer == session.Player1 ? session.Player2 : session.Player1;
                await Clients.Client(session.CurrentPlayer).SendAsync("YourTurn");
                LogUtil.LogMessage("Player AccountId :" + accid + " cell row : " + row + " col : " + col + "value : " + player);

            }
            catch(Exception ex)
            {
                LogUtil.LogFailed(ex);
            }
            
        }
    }
}
