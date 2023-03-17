﻿using Caro.Game.Enum;
using Caro.Game.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Caro.Game.Hubs
{
    public partial class CaroHub
    {
        public async Task JoinSession(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                throw new Exception($"Game session {sessionId} not found");
            }

            if (!String.IsNullOrEmpty(session.Player2))
            {
                throw new Exception($"Game session {sessionId} is full");
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
        }
    }
}