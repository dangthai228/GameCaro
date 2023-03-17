using Caro.Game.DBAccess.DAO;
using Caro.Game.DBAccess.DAOImpl;

namespace Caro.Game.DBAccess.Factory
{
    public class ImplFactoryDAO : AbstractFactoryDAO
    {
        public override IGameDAO CreateGameDAO(AppSettings appsettings)
        {
            var instance = (IGameDAO)new GameDAOImpl();
            instance.appSettings = appsettings;
            return instance;
        }
    }
}
