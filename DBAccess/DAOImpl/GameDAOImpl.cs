using Caro.Game.DBAccess.DAO;
using Caro.Game.DBAccess.Model;
using Caro.Game.Models;
using Caro.Game.Session;
using Caro.Game.Utilts;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;

namespace Caro.Game.DBAccess.DAOImpl
{
    public class GameDAOImpl : IGameDAO
    {
        public override DBResult<long> Authenticated(string username, string password)
        {
            DBResult<long> result = new DBResult<long>();
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[5];
                _params[0] = new MySqlParameter("p_username", username);
                _params[1] = new MySqlParameter("p_password", password);
                _params[2] = new MySqlParameter("p_ResponseStatus", MySqlDbType.Int64) { Direction = ParameterDirection.Output };
                _params[3] = new MySqlParameter("p_ResponseText", MySqlDbType.VarChar) { Direction = ParameterDirection.Output };
                _params[4] = new MySqlParameter("p_accountid", MySqlDbType.Int64) { Direction = ParameterDirection.Output };
                MySqlCommand cmd = new MySqlCommand("sp_AuthenAccount", con);

                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                cmd.ExecuteNonQuery();
                
                result.ResponseStatus = Convert.ToInt32((_params[2].Value.ToString()));
                result.ResponseText = _params[3].Value.ToString();
                result.Data = Convert.ToInt32((_params[4].Value.ToString()));
                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                result.ResponseStatus = -99;
                result.ResponseText = ex.Message.ToString();
                return result;
            }
        }

        public override PlayerCaro GetAccountInfo(long accountID)
        {
            PlayerCaro player = new PlayerCaro();
            MySqlDataReader rd;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[1];
                _params[0] = new MySqlParameter("p_accountId", accountID);
                MySqlCommand cmd = new MySqlCommand("sp_GetAccountInfo", con);
                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                 rd =  cmd.ExecuteReader();
                if(rd.HasRows)
                {
                    while(rd.Read())
                    {
                        player.AccountId = accountID;
                        player.Name = rd.GetString("Name");
                        player.ToTalWin = rd.GetInt32("Win");
                        player.ToTalLose = rd.GetInt32("Lose");
                        player.ToTalDraw = rd.GetInt32("Draw");
                        player.Balance = rd.GetInt32("Gold");
                    }
                }
                rd.Close();
                con.Close();
                return player;
               
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override List<UseItem> getListUseItem(long accountID)
        {
            List<UseItem> _list = new List<UseItem>();
            MySqlDataReader rd;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[1];
                _params[0] = new MySqlParameter("p_accountid", accountID);
                MySqlCommand cmd = new MySqlCommand("sp_GetUseItemInventory", con);
                con.Open(); 
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                 rd = cmd.ExecuteReader();
                if (rd != null)
                {
                    while (rd.Read())
                    {
                        UseItem _item = new UseItem();
                        _item.InventoryId = rd.GetInt32("id");
                        _item.ItemId = rd.GetInt32("iditem");
                        _item.Expired = rd.GetDateTime("expired").ToString();
                        _list.Add(_item);
                    }
                    rd.Close();
                }
                con.Close();
                return _list;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override bool UpdateAfterEndGame(long winnnerID, long LoserId, int betValue)
        {
            bool result = false;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[5];
                _params[0] = new MySqlParameter("p_winnerId", winnnerID);
                _params[1] = new MySqlParameter("p_loserId", LoserId);
                _params[2] = new MySqlParameter("p_betValue", betValue);
                _params[3] = new MySqlParameter("p_ResponseStatus", MySqlDbType.Int64) { Direction = ParameterDirection.Output };
                _params[4] = new MySqlParameter("p_ResponseText", MySqlDbType.VarChar) { Direction = ParameterDirection.Output };
                MySqlCommand cmd = new MySqlCommand("sp_UpdateAfterEndGame", con);
                con.Open();
                
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                cmd.ExecuteNonQuery();

                if (Convert.ToInt32((_params[2].Value.ToString())) == 1)
                {
                    result = true;
                }
                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                result = false;
                LogUtil.LogFailed(ex);
            }
            return result;
        }

        public override bool UpdateDraw(long playerID1, long PlayerID2, int betValue)
        {
            bool result = false;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[5];
                _params[0] = new MySqlParameter("p_account1", playerID1);
                _params[1] = new MySqlParameter("p_account2", PlayerID2);
                _params[2] = new MySqlParameter("p_betValue", betValue);
                _params[3] = new MySqlParameter("p_ResponseStatus", MySqlDbType.Int64) { Direction = ParameterDirection.Output };
                _params[4] = new MySqlParameter("p_ResponseText", MySqlDbType.VarChar) { Direction = ParameterDirection.Output };
                MySqlCommand cmd = new MySqlCommand("sp_UpdateGameDraw", con);
                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                cmd.ExecuteNonQuery();
                   
                if (Convert.ToInt32((_params[2].Value.ToString())) == 1)
                {
                    result = true;
                }
                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                LogUtil.LogFailed(ex);
                result = false;
            }
            return result;
        }

        public override List<Inventory> getInventory(long accountID)
        {
            List<Inventory> _list = new List<Inventory>();
            MySqlDataReader rd;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[1];
                _params[0] = new MySqlParameter("p_accountid", accountID);
                MySqlCommand cmd = new MySqlCommand("sp_GetInventory", con);
                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while(rd.Read())
                    {
                        Inventory _item = new Inventory();
                        _item.InventoryId = rd.GetInt32("id");
                        _item.Name = rd.GetString("name");

                        _item.Expired = rd.IsDBNull("expired") ? "" : rd.GetDateTime("expired").ToString();

                        
                        _item.Description = rd.GetString("description");
                        _item.TimeBuy = rd.GetString("time").ToString();
                        _list.Add(_item);
                    }   
                }
                rd.Close();
                con.Close();
                return _list;
            }
            catch (Exception ex)
            {
                LogUtil.LogFailed(ex);
                return null;
            }
        }

        public override List<ShopItem> GetShop()
        {
            List<ShopItem> _list = new List<ShopItem>();
            MySqlDataReader rd;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                MySqlCommand cmd = new MySqlCommand("sp_Getshop", con);
                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        ShopItem _item = new ShopItem();
                        _item.ItemId = rd.GetInt32("id");
                        _item.Name = rd.GetString("name");
                        _item.Description = rd.GetString("description");
                        _item.Gold = rd.GetInt32("gold");
                        _item.BrrGold = rd.GetInt32("brrgold");
                        _item.Type = rd.GetString("type");
                        _item.IsBought = rd.GetInt32("isbought");
                        _item.IsBrr = rd.GetInt32("Isborrowed");
                        _list.Add(_item);
                    }
                }
                rd.Close();
                con.Close();
                return _list;
            }
            catch (Exception ex)
            {
                LogUtil.LogFailed(ex);
                return null;
            }
        }

        public override bool BuyItem(long accid, int itemid, int daybrr)
        {
            bool result = false;
            MySqlConnection con = new MySqlConnection(this.appSettings.ConnectionString.ToString());
            try
            {
                var _params = new MySqlParameter[5];
                _params[0] = new MySqlParameter("p_accountId", accid);
                _params[1] = new MySqlParameter("p_ItemId", itemid);
                _params[2] = new MySqlParameter("p_Days", daybrr);
                _params[3] = new MySqlParameter("p_ResponseStatus", MySqlDbType.Int64) { Direction = ParameterDirection.Output };
                _params[4] = new MySqlParameter("p_ResponseText", MySqlDbType.VarChar) { Direction = ParameterDirection.Output };
                MySqlCommand cmd = new MySqlCommand("BuyItem", con);
                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(_params);
                cmd.ExecuteNonQuery();
                if (Convert.ToInt32(_params[3].Value.ToString())== 1)
                {
                    result = true;
                }
                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                LogUtil.LogFailed(ex);
                result = false;
            }
            return result;
        }
    }
}
