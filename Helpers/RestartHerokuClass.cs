using IsBookingOpenedAPI.Entities;
using Microsoft.Extensions.Options;

namespace IsBookingOpenedAPI.Helpers
{
    public class RestartHerokuClass
    {
        private readonly IConfiguration _appSettings;
        private readonly ILogger<RestartHerokuClass> _logger;
        private static readonly HttpClient client = new HttpClient();

        public RestartHerokuClass(IConfiguration config, ILogger<RestartHerokuClass> logger)
        {
            _appSettings = config;
            _logger = logger;
        }
        public int RestartHeroku()
        {
            try
            {
                //HerokuDeleteDyno();
                //HerokuStart();
                HttpGet(_appSettings["IsBookingOpenedAPIURL"]+"api/polling");
            }
            catch
            {
                return -1;
            }
            return 0;
        }

        public void RestartShutDown(Task ex)
        {
            _logger.LogError($"RestartShutDown {ex.Exception}");
        }

        private void HerokuDeleteDyno()
        {
            try
            {
                _logger.LogInformation($"Begin HerokuDeleteDyno");

                DeleteHeroku delBody = new DeleteHeroku();
                delBody.apps = _appSettings["AppName"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"98\", \"Google Chrome\";v=\"98\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
                    httpClient.DefaultRequestHeaders.Add("accept", "application/vnd.heroku+json; version=3.cedar-acm");
                    httpClient.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9");
                    httpClient.DefaultRequestHeaders.Add("origin", "https://dashboard.heroku.com");
                    httpClient.DefaultRequestHeaders.Add("referer", "https://dashboard.heroku.com/");
                    httpClient.DefaultRequestHeaders.Add("x-heroku-requester", "dashboard");
                    httpClient.DefaultRequestHeaders.Add("x-origin", "https://dashboard.heroku.com");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _appSettings["Token"] + "");
                    httpClient.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
                    using (var response = httpClient.DeleteAsync("https://api.heroku.com/apps/" + _appSettings["AppID"] + "/dynos").Result)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error HerokuDeleteDyno {ex}");
                throw;
            }
        }

        private void HerokuStart()
        {
            try
            {
                _logger.LogInformation($"Begin HerokuStart");

                string appName = _appSettings["AppName"];
                string jsonBody =
                "{\"source\":\"dashboard\",\"event\":\"App Dynos Restarted\",\"userId\":\"e1df11e3-e3ef-4e00-b47a-1c4b4307ea34\"," +
                "\"properties\":{\"route\":\"protected.app.resources.index\",\"effectiveNetworkType\":\"4g\"},\"page\":" +
                "{\"url\":\"https://dashboard.heroku.com/apps/" + appName + "/resources\",\"path\":\"/apps/" + appName + "/resources\"," +
                "\"search\":\"\",\"title\":\"" + appName + " · Resources | Heroku\",\"referrer\":\"https://dashboard.heroku.com/\"}}";

                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                string data = System.Convert.ToBase64String(plainTextBytes);


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"98\", \"Google Chrome\";v=\"98\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
                    httpClient.DefaultRequestHeaders.Add("origin", "https://dashboard.heroku.com");
                    httpClient.DefaultRequestHeaders.Add("accept", "*/*");
                    httpClient.DefaultRequestHeaders.Add("referer", "https://dashboard.heroku.com/");
                    httpClient.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
                    using (var response = httpClient.GetAsync("https://backboard.heroku.com/hamurai?data=" + data).Result)
                    {
                        string responseData = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error HerokuDeleteDyno {ex}");
                throw;
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
    }
}
