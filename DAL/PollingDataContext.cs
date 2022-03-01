using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using IsBookingOpenedAPI.Entities;

namespace WebAPIPostgresql.DAL
{
    public interface IPollingDataContext
    {
        public int UpdateTriggerStatus(Trigger triggerObj);
        public int DeleteOldTriggers(string triggerDate);
        public IEnumerable<Trigger> GetAllPendingTriggers();
        public bool DisposeConnection(bool status);

    }

    public class PollingDataContext : IPollingDataContext
    {
        private readonly AppSettings _appSettings;
        private NpgsqlConnection _npgSqlCon = null;
        private NpgsqlCommand _npsqlCmd = null;
        private NpgsqlDataReader _npSqlRdr = null;
        private bool disposedValue = false;
        private readonly ILogger<PollingDataContext> _logger;


        public PollingDataContext(IOptions<AppSettings> appSettings, ILogger<PollingDataContext> logger)
        {
            _appSettings = appSettings.Value;
            if (_npgSqlCon == null)
                _npgSqlCon = new NpgsqlConnection(_appSettings.ConnectionString);
            if (_npgSqlCon.State != ConnectionState.Open)
                _npgSqlCon.Open();
            _logger = logger;
        }

        public int UpdateTriggerStatus(Trigger triggerObj)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation("Begin UpdateTriggerStatus ");

                var updateSQL = String.Format(@$"UPDATE trigger SET 
                                            trigger_status = {(Int16)triggerObj.trigger_status}, 
                                            updated_on = '{triggerObj.updated_on.ToString("yyyy-MM-dd HH':'mm':'ss")}' 
                                            WHERE trigger_id = {triggerObj.trigger_id};");

                _logger.LogInformation($"updateSQL {updateSQL}");


                _npsqlCmd = new NpgsqlCommand(updateSQL, _npgSqlCon);

                resp = _npsqlCmd.ExecuteNonQuery();

                _logger.LogInformation($"End UpdateTriggerStatus {resp}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured in UpdateTriggerStatus {ex}");
                throw;
            }
            finally
            {
                DisposeNpSqlCmd();
            }
            return resp;
        }

        public int DeleteOldTriggers(string triggerDate)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation($"Begin Delete Trigger triggerDate : {triggerDate}");


                var selectSQl = String.Format(@$"DELETE FROM trigger WHERE  trigger_date <= '{triggerDate}';");


                _npsqlCmd = new NpgsqlCommand(selectSQl, _npgSqlCon);

                resp = _npsqlCmd.ExecuteNonQuery();

                _logger.LogInformation($"Success SelectTrigger {resp}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed SelectTrigger {ex}");
                throw;
            }
            finally
            {
                DisposeNpSqlCmd();
            }

            return resp;
        }

        public IEnumerable<Trigger> GetAllPendingTriggers()
        {
            List<Trigger> resp = new List<Trigger>();
            try
            {
                _logger.LogInformation($"Begin SelectAllTriggers");


                var selectSQl = String.Format(@$"SELECT * FROM trigger WHERE trigger_status = {(Int16)TriggerStatus.Pending} ;");


                _npsqlCmd = new NpgsqlCommand(selectSQl, _npgSqlCon);

                _npSqlRdr = _npsqlCmd.ExecuteReader();
                while (_npSqlRdr.Read())
                {
                    Trigger triggerObj = new Trigger();
                    triggerObj.trigger_id = Int32.Parse(_npSqlRdr["trigger_id"].ToString());
                    triggerObj.movie_name = _npSqlRdr["movie_name"].ToString();
                    triggerObj.theater_name = _npSqlRdr["theater_name"].ToString();
                    triggerObj.theater_id = _npSqlRdr["theater_id"].ToString();
                    triggerObj.trigger_url = _npSqlRdr["trigger_url"].ToString();
                    triggerObj.trigger_status = (TriggerStatus)Int16.Parse(_npSqlRdr["trigger_status"].ToString());
                    triggerObj.trigger_date = _npSqlRdr["trigger_date"].ToString();
                    triggerObj.trigger_time = _npSqlRdr["trigger_time"].ToString();
                    resp.Add(triggerObj);
                }
                _logger.LogInformation($"Success SelectTrigger {resp?.Count}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed SelectTrigger {ex}");
                throw;
            }
            finally
            {
                DisposeNpSqlCmd();
            }

            return resp;
        }

        public bool DisposeConnection(bool status)
        {
            try
            {
                if (status)
                {
                    if (this._npgSqlCon.State == ConnectionState.Open)
                        _npgSqlCon.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed DisposeConnection {ex}");
                return false;
            }
        }

        private void DisposeNpSqlCmd()
        {
            if (_npsqlCmd != null)
                _npsqlCmd.Dispose();
            if (_npSqlRdr != null)
                _npSqlRdr.Close();
        }
    }
}
