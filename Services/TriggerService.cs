using IsBookingOpenedAPI.DAL;
using IsBookingOpenedAPI.Entities;

namespace IsBookingOpenedAPI.Services
{
    public interface ITriggerService
    {
        public int AddUpdateTrigger(Trigger triggerObj);
        public IEnumerable<Trigger> GetAllTriggers();
        public int DeleteTrigger(int triggerId);


    }
    public class TriggerService: ITriggerService
    {
        private readonly ITriggerDataContext _triggerDataContext;

        public TriggerService(ITriggerDataContext triggerDataContext)
        {
            _triggerDataContext = triggerDataContext;
        }

        public int AddUpdateTrigger(Trigger triggerObj)
        {
            try
            {
                if (triggerObj.trigger_id <= 0)
                {
                   return _triggerDataContext.CreateTrigger(triggerObj);
                }
                if (triggerObj.trigger_id > 0)
                {
                    if (_triggerDataContext.SelectTrigger(triggerObj.trigger_id) != triggerObj.trigger_id)
                    {
                        return (int)TriggerResponseStatus.RecordNotExist;
                    }
                    return _triggerDataContext.UpdateTrigger(triggerObj);
                }
                else
                    return -1;

            }
            catch (Exception)
            {
                throw;
            }           
        }

        public IEnumerable<Trigger> GetAllTriggers()
        {
            try
            {
                return _triggerDataContext.GetAllTriggers();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteTrigger(int triggerId)
        {
            try
            {
                return _triggerDataContext.DeleteTriggerById(triggerId);

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
