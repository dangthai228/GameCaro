using System;

namespace Caro.Game.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Expired { get; set; }
        public string TimeBuy { get; set; }
    }
}
