using Caro.Game.Enum;
using System.Collections.Generic;

namespace Caro.Game.Models
{
    public class PlayerCaro
    {
        public long AccountId { get; set; }
        public string Name { get; set; }
        public int ToTalWin { get; set; }
        public int ToTalLose { get; set; }
        public int ToTalDraw { get; set; }
        public int Balance { get; set; }
        public List<UseItem> listUseItem { get;set; }
        public PlayerState Status { get; set; }

    }
}
