using IsBookingOpenedAPI.Entities;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace IsBookingOpenedAPI.DAL
{
    public interface ITriggerDataContext
    {
        public int CreateTrigger(Trigger triggerObj);
        public int UpdateTrigger(Trigger triggerObj);
        public int SelectTrigger(int triggerID);
        public IEnumerable<Trigger> GetAllTriggers();
        public int DeleteTriggerById(int triggerID);

    }
    public class TriggerDataContext : ITriggerDataContext,IDisposable
    {
        private readonly AppSettings _appSettings;
        private NpgsqlConnection _npgSqlCon = null;
        private NpgsqlCommand _npsqlCmd = null;
        private NpgsqlDataReader _npSqlRdr = null;
        private bool disposedValue = false;
        private readonly ILogger<TriggerDataContext> _logger;



        public TriggerDataContext(IOptions<AppSettings> appSettings, ILogger<TriggerDataContext> logger)
        {
            _appSettings = appSettings.Value;
            if (_npgSqlCon == null)
                _npgSqlCon = new NpgsqlConnection(_appSettings.ConnectionString);
            if (_npgSqlCon.State != ConnectionState.Open)
                _npgSqlCon.Open();
            _logger = logger;
        }

        public int CreateTrigger(Trigger triggerObj)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation($"Begin CreateTrigger trigger_url: {triggerObj?.trigger_url}");
                var insertSQL = String.Format(@$"INSERT INTO trigger (movie_name,theater_name,theater_id,trigger_url,
                                                trigger_status,trigger_date,
                                                trigger_time,created_on,updated_on)
                                            VALUES ('{triggerObj.movie_name}','{triggerObj.theater_name}',
                                            '{triggerObj.theater_id}',
                                            '{triggerObj.trigger_url}',{(Int16)triggerObj.trigger_status},
                                            '{triggerObj.trigger_date}','{triggerObj.trigger_time}',
                                            '{triggerObj.created_on.ToString("yyyy-MM-dd HH':'mm':'ss")}',
                                            '{triggerObj.updated_on.ToString("yyyy-MM-dd HH':'mm':'ss")}'); ");

                _npsqlCmd = new NpgsqlCommand(insertSQL, _npgSqlCon);

                resp = _npsqlCmd.ExecuteNonQuery();
                _logger.LogInformation($"Success CreateTrigger {resp}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed CreateTrigger {ex}");
                throw;
            }
            finally
            {
                DisposeNpSqlCmd();
            }

            return resp;
        }

        public int UpdateTrigger(Trigger triggerObj)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation($"Begin UpdateTrigger trigger_url: {triggerObj?.trigger_url}");


                var updateSQL = String.Format(@$"UPDATE trigger SET 
                                            movie_name = '{triggerObj.movie_name}',
                                            theater_name = '{triggerObj.theater_name}',
                                            trigger_id = '{triggerObj.trigger_id}',
                                            trigger_url = '{triggerObj.trigger_url}',
                                            trigger_status = {(Int16)triggerObj.trigger_status}, 
                                            trigger_date = '{triggerObj.trigger_date}', 
                                            trigger_time = '{triggerObj.trigger_time}', 
                                            updated_on = '{triggerObj.updated_on.ToString("yyyy-MM-dd HH':'mm':'ss")}' 
                                            WHERE trigger_id = {triggerObj.trigger_id};");


                _npsqlCmd = new NpgsqlCommand(updateSQL, _npgSqlCon);

                resp = _npsqlCmd.ExecuteNonQuery();
                _logger.LogInformation($"Success UpdateTrigger {resp}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed UpdateTrigger {ex}");
                throw;
            }
            finally
            {
                DisposeNpSqlCmd();
            }

            return resp;
        }

        public int SelectTrigger(int triggerID)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation($"Begin SelectTrigger triggerID: {triggerID}");


                var selectSQl = String.Format(@$"SELECT trigger_id FROM trigger WHERE  trigger_id = {triggerID};");


                _npsqlCmd = new NpgsqlCommand(selectSQl, _npgSqlCon);

                _npSqlRdr = _npsqlCmd.ExecuteReader();
                while (_npSqlRdr.Read())
                {
                    resp = Int32.Parse(_npSqlRdr[0].ToString());
                }
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

        public int DeleteTriggerById(int triggerID)
        {
            int resp = -1;
            try
            {
                _logger.LogInformation($"Begin Delete Trigger triggerID : {triggerID}");


                var selectSQl = String.Format(@$"DELETE FROM trigger WHERE  trigger_id = {triggerID};");


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

        public IEnumerable<Trigger> GetAllTriggers()
        {
            List<Trigger> resp = new List<Trigger>();
            try
            {
                _logger.LogInformation($"Begin GetAllTriggers");


                var selectSQl = String.Format(@$"SELECT * FROM trigger ;");


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
                _logger.LogInformation($"Success GetAllTriggers {resp?.Count}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed GetAllTriggers {ex}");
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    DisposeConnection(true);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TriggerDataContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
