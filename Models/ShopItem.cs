namespace Caro.Game.Models
{
    public class ShopItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Gold { get; set; }
        public int BrrGold { get; set;}
        public string Type { get; set; }
        public int IsBought { get; set; }
        public int IsBrr { get; set; }
    }
}
