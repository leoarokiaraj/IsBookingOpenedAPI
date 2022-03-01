namespace IsBookingOpenedAPI.Entities
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }
        public string IsBookingOpenedServiceURL { get; set; }
        public string IsBookingOpenedAPIURL { get; set; }
        public int PollingDelay { get; set; }
        public int HerokuPollingDelay { get; set; }
        public string DiscordAPI { get; set; }
        public string AppName { get; set; }
        public string AppID { get; set; }
        public string Token { get; set; }
        public string LastHerokuRestart { get; set; }
    }
}
