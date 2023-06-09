﻿using Caro.Game.Enum;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (!_player.TryGetValue(Context.ConnectionId, out var player))
                {
                    return;
                }
                long accidDisconnect = player.AccountId;
                if (player.Status == PlayerState.Online)
                {
                    _player.TryRemove(Context.ConnectionId.ToString(), out _);
                }
                else if (player.Status == PlayerState.InRoom || player.Status == PlayerState.Ready_To_Play)
                {

                    string sessionId = string.Empty;
                    GameSession session = null;
                    foreach (var _gamesession in _sessions.Values)
                    {
                        if (_gamesession.Player1 == Context.ConnectionId || _gamesession.Player2 == Context.ConnectionId)
                        {
                            sessionId = _gamesession.SessionId;
                            session = _gamesession;
                            break;
                        }
                    }

                    string connectionOpposite = String.Empty;
                    if (Context.ConnectionId == session.Player1)
                    {
                        connectionOpposite = session.Player2;
                    }
                    else if (Context.ConnectionId == session.Player2)
                    {
                        connectionOpposite = session.Player1;
                    }

                    // Xoa connectID  trong gamesession
                    if (Context.ConnectionId == session.Player1)
                    {
                        if (!String.IsNullOrEmpty(connectionOpposite))
                        {
                            session.Player1 = connectionOpposite;
                            session.Player2 = String.Empty;
                            session.CurrentPlayer = connectionOpposite;
                            _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);

                            // update state opposite
                            if (_player.TryGetValue(connectionOpposite, out var opposite))
                            {
                                opposite.Status = PlayerState.InRoom;
                                _player.AddOrUpdate(connectionOpposite, opposite, (_k, _v) => _v = opposite);
                            }
                        }
                        else
                        {
                            _sessions.TryRemove(sessionId, out _);
                        }
                    }
                    else
                    {
                        session.Player2 = String.Empty;
                        _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);
                    }

                    await Clients.OthersInGroup(sessionId).SendAsync("DisconnectedWhileInRoom", Context.ConnectionId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
                }
                else if (player.Status == PlayerState.InGame)
                {

                    string sessionId = string.Empty;
                    GameSession session = null;
                    // search sessionId va session theo ConnectionId
                    foreach (var _gamesession in _sessions.Values)
                    {
                        if (_gamesession.Player1 == Context.ConnectionId || _gamesession.Player2 == Context.ConnectionId)
                        {
                            sessionId = _gamesession.SessionId;
                            session = _gamesession;
                            break;
                        }
                    }

                    string connectionOpposite = String.Empty;
                    if (Context.ConnectionId.Equals(session.Player1))
                    {
                        connectionOpposite = session.Player2;
                    }
                    else
                    {
                        connectionOpposite = session.Player1;
                    }
                    bool IsUpdate = updateAfterEndGame(session, false);
                    // thong bao thang cuoc cho doi thu 
                    await Clients.OthersInGroup(sessionId).SendAsync("DisconnectedWhileInGame");
                    // await Clients.OthersInGroup(sessionId).SendAsync("UpdateAfterEnd", IsUpdate);

                    // update state opposite
                    if (_player.TryGetValue(connectionOpposite, out var opposite))
                    {
                        opposite.Status = PlayerState.InRoom;
                        _player.AddOrUpdate(connectionOpposite, opposite, (_k, _v) => _v = opposite);
                    }

                    // Xoa connectID  trong gamesession
                    if (Context.ConnectionId == session.Player1)
                    {
                        session.Player1 = connectionOpposite;
                        session.Player2 = String.Empty;
                        session.CurrentPlayer = connectionOpposite;
                        _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);
                    }
                    else
                    {
                        session.Player2 = String.Empty;
                        _sessions.AddOrUpdate(sessionId, session, (k, v) => v = session);
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
                }
                _inventory.TryRemove(accidDisconnect, out _);
                _player.TryRemove(Context.ConnectionId, out _);
                LogUtil.LogMessage("Player AccountId : "+ accidDisconnect+ "  has disconnected");
            }
            catch(Exception ex) {
                LogUtil.LogFailed(ex);
            }
            
            return;
        }

        [AllowAnonymous]
        public override async Task OnConnectedAsync()
        {
            try
            {
                bool isOnline = false;
                long accid = AccountClaim.getAccountID(Context);
                PlayerCaro playercaro = gameDAO.GetAccountInfo(accid);

                playercaro.listUseItem = gameDAO.getListUseItem(accid);
                playercaro.Status = PlayerState.Online;


                foreach (var player in _player.Values)
                {
                    if (player.AccountId == accid)
                    {
                        isOnline = true;
                        break;
                    }
                }
                if (isOnline)
                {
                    await Clients.Caller.SendAsync("ConnectFailed");
                    LogUtil.LogMessage("Connect to signalR Failed due to accountId = " + accid + " is Already Online ");
                    return;
                }
                else
                {
                    if (!_player.TryAdd(Context.ConnectionId, playercaro))
                    {
                        LogUtil.LogMessage("Can not add playercaro to dictionary : " + JsonConvert.SerializeObject(playercaro));
                        return;
                    }
                    List<Inventory> listinventory = gameDAO.getInventory(accid);
                    if (!_inventory.TryAdd(accid, listinventory))
                    {
                        LogUtil.LogMessage("Can not add list inventory to dictionary : " + JsonConvert.SerializeObject(listinventory));
                        return;
                    }
                }
                await Clients.Caller.SendAsync("ConnectSucess");
                LogUtil.LogMessage("AccountId : " + accid + " connect to SignalR success !!!");
            }
            catch(Exception ex) 
            {
                LogUtil.LogFailed(ex);
            }
            
            return ;
        }
    }
}
