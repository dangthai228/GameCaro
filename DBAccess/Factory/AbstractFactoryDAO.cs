using Caro.Game.DBAccess.DAO;
using System;

namespace Caro.Game.DBAccess.Factory
{
    public abstract class AbstractFactoryDAO
    {
        public static AbstractFactoryDAO Instance() 
        {
            try
            {
                return (AbstractFactoryDAO)new ImplFactoryDAO();
            }
            catch (Exception)
            {
                throw new Exception("Couldn't create AbstractDAOFactory: ");
            }
        }

        public abstract IGameDAO CreateGameDAO(AppSettings appsettings);
    }
}
