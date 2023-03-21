using Caro.Game.DBAccess.Model;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using Org.BouncyCastle.Asn1.Cms;
using System.Collections.Generic;

namespace Caro.Game.DBAccess.DAO
{
    public abstract class IGameDAO
    {
        public AppSettings appSettings { get; set; }


        // check user and return account ID
        public abstract DBResult<long> Authenticated(string username, string password);

        public abstract PlayerCaro GetAccountInfo(long accountID);

        public abstract bool UpdateAfterEndGame(long winnnerID,long LoserId, int betValue);

        public abstract bool UpdateDraw(long playerID1, long PlayerID2, int betValue);

        public abstract List<UseItem> getListUseItem (long accountID);

        public abstract List<Inventory> getInventory(long accountID);
    }
}
