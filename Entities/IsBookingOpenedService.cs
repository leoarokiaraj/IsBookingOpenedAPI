namespace IsBookingOpenedAPI.Entities
{
    public class IsBookingOpenedService
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public List<ShowDetails> showDetails { get; set; }
    }


    public class ShowDetails
    {
        public string data_id { get; set; } 
        public List<string> data_date_time { get; set; }
    }

    public class DiscordBody
    {
        public string content { get; set; }
    }

    public class DeleteHeroku
    {
        public string apps {get;set;}
    }
}

