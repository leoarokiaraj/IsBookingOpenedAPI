using Microsoft.Extensions.Options;
using WebAPIPostgresql.DAL;
using IsBookingOpenedAPI.Entities;
using IsBookingOpenedAPI.DAL;
using IsBookingOpenedAPI.Helpers;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIPostgresql.Services
{
    public interface IPollingService
    {
        public string StartPollingService();

    }
    public class PollingService : IPollingService
    {
        private IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PollingService> _logger;
        private static readonly HttpClient client = new HttpClient();


        public PollingService(IOptions<AppSettings> appSettings, IServiceProvider provider, ILogger<PollingService> logger)
        {
            _appSettings = appSettings.Value;
            _provider = provider;
            _logger = logger;
        }
        public string StartPollingService()
        {
            Task listener = null;
            if (PollingSingleton.pollingListener == null)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                IPollingDataContext _pollingDataContext = _provider.GetService<IPollingDataContext>();

                listener = Task.Factory.StartNew(() => {
                    while (true)
                    {
                        int pollStatus = PollingOnTrigger(_pollingDataContext);
                        HttpGet(_appSettings.IsBookingOpenedAPIURL+"api/polling");
                        if (pollStatus < 0)
                            break;

                        Thread.Sleep(_appSettings.PollingDelay);
                        if (token.IsCancellationRequested)
                            break;
                    }
                    
                }
                , token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                listener?.ContinueWith(t => ShutDown(t, _pollingDataContext));

                PollingSingleton.pollingListener = listener;
            }
            


            return "Polling service started " + DateTime.Now.ToString();
        }

        private int PollingOnTrigger(IPollingDataContext _pollingDataContext)
        {
            try
            {
                IEnumerable<Trigger> triggers = _pollingDataContext.GetAllPendingTriggers();
                if (triggers == null || triggers?.Count() <= 0)
                {
                    return 0;
                }
                foreach (var triggerItem in triggers)
                {
                    string getURL = @$"{_appSettings.IsBookingOpenedServiceURL}api/show-opened?pagePath={ triggerItem.trigger_url}";

                    var resp = HttpGet(getURL);
                    if (resp == String.Empty)
                    {
                        continue;
                    }
                    IsBookingOpenedService showOpendObj = JsonConvert.DeserializeObject<IsBookingOpenedService>(resp);

                    if (showOpendObj == null || showOpendObj?.StatusCode != 200
                            || showOpendObj?.showDetails == null)
                    {
                        continue;
                    }
                    foreach (var showDettail in showOpendObj?.showDetails)
                    {
                        List<string> dateTime = new List<String>();
                        if (showDettail?.data_id != triggerItem?.theater_id)
                        {
                            continue;
                        }
                        if (showDettail?.data_date_time == null || triggerItem?.trigger_time == null)
                        {
                            continue;
                        }
                        foreach (var date in showDettail?.data_date_time)
                        {
                            if (DateTime.Parse(date) > (DateTime.Parse(triggerItem?.trigger_time)))
                            {
                                dateTime.Add(date);
                                triggerItem.trigger_status = TriggerStatus.Success;

                            }
                        }
                        if (dateTime.Count > 0 && triggerItem.trigger_status == TriggerStatus.Success)
                        {
                            UpdateDiscord(triggerItem?.movie_name + " in " + triggerItem?.theater_name + " at " + triggerItem?.trigger_date + " " + String.Join(',', dateTime));
                            _pollingDataContext.UpdateTriggerStatus(triggerItem);
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Polling service ShutDown {ex}");
                return -1;
            }


        }

        private string HttpGet(string uri)
        {
            try
            {
                return client.GetStringAsync(uri).Result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
        }

        private HttpResponseMessage UpdateDiscord(string message)
        {
            try
            {
                _logger.LogInformation($"Begin UpdateDiscord");

                HttpResponseMessage discordResp = new HttpResponseMessage();
                
                DiscordBody discordBody = new DiscordBody();

                discordBody.content = "@everyone Booking opened for the movie "+ message;

                discordResp = client.PostAsJsonAsync(_appSettings.DiscordAPI,
                   discordBody).Result;

                return discordResp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured in UpdateDiscord {ex}");

                throw;
            }
        }


        private void ShutDown(Task t, IPollingDataContext pollingDataContext)
        {
            PollingSingleton.pollingListener = null;
            pollingDataContext.DisposeConnection(true);
            _logger.LogInformation($"Polling service ShutDown {t.Exception}");
        }
    }
}
