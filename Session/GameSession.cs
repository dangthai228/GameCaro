using System;

namespace Caro.Game.Session
{
    public class GameSession
    {
        public string NameTable { get; set; }
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string[,] Board { get; set; } = new string[15, 15];
        public string CurrentPlayer { get; set; }
        public int betValue { get; set; }
    }
}
