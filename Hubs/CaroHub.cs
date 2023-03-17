﻿using Caro.Game.DBAccess.DAO;
using Caro.Game.DBAccess.Factory;
using Caro.Game.Enum;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Caro.Game.Hubs
{
    [Authorize]
    public partial class CaroHub : Hub
    {
        //key : SessionId(guid) value la GameSession 
        private static readonly ConcurrentDictionary<string, GameSession> _sessions =
            new ConcurrentDictionary<string, GameSession>();

        // key la connectionID va value la PlayerCaro 
        private static readonly ConcurrentDictionary<string, PlayerCaro>  _player = new ConcurrentDictionary<string, PlayerCaro>();


        private static readonly ConcurrentDictionary<long , List<Inventory> > _inventory = new ConcurrentDictionary<long , List<Inventory> >();

        private readonly IGameDAO gameDAO = AbstractFactoryDAO.Instance().CreateGameDAO(Startup.AppSettings);

        private bool updateDraw(GameSession session)
        {
            _player.TryGetValue(session.Player1,out var player1);
            _player.TryGetValue(session.Player2, out var player2);
            bool res = gameDAO.UpdateDraw(player1.AccountId, player2.AccountId, session.betValue);
            return res;
        }

        public bool updateAfterEndGame(GameSession session, bool IsWinner)
        {
            bool res = false;
            if (session == null) 
            {
                return res;
            }
            else
            {
                string connectionOpposite = String.Empty;
                long accountId = AccountClaim.getAccountID(Context);
                if (Context.ConnectionId.Equals(session.Player1))
                {
                    connectionOpposite = session.Player2;
                }
                else
                {
                    connectionOpposite = session.Player1;
                }

                if(!_player.TryGetValue(connectionOpposite, out var otherPlayer))
                {
                    return false;
                }

                if (IsWinner)
                { 
                     res = gameDAO.UpdateAfterEndGame(accountId, otherPlayer.AccountId, session.betValue);
                }
                else
                {
                    res = gameDAO.UpdateAfterEndGame(otherPlayer.AccountId, accountId, session.betValue);
                }
                // cap nhat lai PLayerCaro 
                ReAddPlayerCaro(session.Player1);
                ReAddPlayerCaro(session.Player2);
                return res;
            }
        }

        private void ReAddPlayerCaro(string _connnectionId) 
        {
            if(_player.ContainsKey(_connnectionId))
            {
                _player.TryGetValue(_connnectionId, out var player);
                long accountID = player.AccountId;
                PlayerCaro newplayer = gameDAO.GetAccountInfo(accountID);
                newplayer.Status = PlayerState.InRoom;
                _player.AddOrUpdate(_connnectionId,newplayer , (k,v)=> v = newplayer);
            }
        }

        private bool CheckWin(string[,] board, string player)
        {
            // check rows
            for (int row = 0; row < 15; row++)
            {
                int count = 0;
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] != player)
                    {
                        count = 0;
                        break;
                    }
                    else
                    {
                        count++;
                        if (count == 5)
                        {
                            return true;
                        }
                    }
                }

                
            }

            // check columns
            for (int col = 0; col < 15; col++)
            {
                int count = 0;
                for (int row = 0; row < 15; row++)
                {
                    if (board[row, col] != player)
                    {
                        count = 0;
                        break;
                    }
                    else
                    {
                        count++;
                        if (count == 5)
                        {
                            return true;
                        }
                    }
                }
            }

            // check diagonal

            for (int i = 0; i < 15; i++)
            {
                int count = 0;
                if (board[i, i] != player)
                {
                    count = 0;
                    break;
                }
                else
                {
                    count++;
                    if(count ==5 )
                    {
                        return true;
                    }
                }
            }


            // check anti-diagonal


            for (int i = 0; i < 15; i++)
            {
                int count = 0;
                if (board[i, 14 - i] != player)
                {
                    count = 0;
                    break;
                }
                else
                {
                    count++;
                    if (count == 5)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckDraw(string[,] board)
        {
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

       
    }
}