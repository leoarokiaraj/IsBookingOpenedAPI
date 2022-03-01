namespace IsBookingOpenedAPI.Entities
{
    public class Trigger
    {
        public int trigger_id { get; set; }
        public string movie_name { get; set; }
        public string theater_name { get; set; }
        public string theater_id { get; set; }        
        public string trigger_url { get; set; }
        public TriggerStatus trigger_status { get; set; }
        public string trigger_date { get; set; }
        public string trigger_time { get; set; }
        public DateTime created_on { 
            get {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
        }
        public DateTime updated_on {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
        }

    }

    public enum TriggerResponseStatus
    {
        Success = 0,
        Failure = -1,
        RecordNotExist = -2
    }

    public enum TriggerStatus
    {
        Pending = 0,
        Success = 1,
        Failure = 2,
    }
}
