using Microsoft.AspNetCore.Diagnostics;
using IsBookingOpenedAPI.Entities;
using Serilog;
using IsBookingOpenedAPI.Services;
using IsBookingOpenedAPI.DAL;
using WebAPIPostgresql.Services;
using WebAPIPostgresql.DAL;
using IsBookingOpenedAPI.Helpers;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

{
    var services = builder.Services;
    var env = builder.Environment;

   int iPort = 7000;
    // use sql server db in production and sqlite db in development
    if (env.IsProduction())
    {
        var port = Environment.GetEnvironmentVariable("PORT");
        int.TryParse(port, out iPort);
        builder.WebHost.ConfigureKestrel(options => {
            options.ListenAnyIP(iPort);
        });
        builder.Configuration.AddJsonFile(@"./AppSettings/appsettings.json");

        IConfiguration config = builder.Configuration.GetSection("AppSettings");
        var postgreURL = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (postgreURL != null)
            config["ConnectionString"] = "Host=" + postgreURL.Split(':')[2].Split('@')[1] + ";Username=" + postgreURL.Split(':')[1].Replace("/", "") + ";Password=" + postgreURL.Split(':')[2].Split('@')[0] + ";Database=" + postgreURL.Split(':')[3].Split('/')[1] + "";
        else
            config["ConnectionString"] = Environment.GetEnvironmentVariable("ConnectionString");
        config["IsBookingOpenedServiceURL"] = Environment.GetEnvironmentVariable("IsBookingOpenedServiceURL");
        config["PollingDelay"] = Environment.GetEnvironmentVariable("PollingDelay");
        config["DiscordAPI"] = Environment.GetEnvironmentVariable("DiscordAPI");

        config["AppName"] = Environment.GetEnvironmentVariable("AppName");
        config["HerokuPollingDelay"] = Environment.GetEnvironmentVariable("HerokuPollingDelay");
        config["AppID"] = Environment.GetEnvironmentVariable("AppID");
        config["Token"] = Environment.GetEnvironmentVariable("Token");
        config["IsBookingOpenedAPIURL"] = Environment.GetEnvironmentVariable("IsBookingOpenedAPIURL");

    }
    else
    {
        builder.Configuration.AddJsonFile(@"./AppSettings/appsettings.json");
        IConfiguration config = builder.Configuration.GetSection("AppSettings");
        var postgreURL = config["PostgreURL"];
        config["ConnectionString"] = "Host=" + postgreURL.Split(':')[2].Split('@')[1] + ";Username=" + postgreURL.Split(':')[1].Replace("/", "") + ";Password=" + postgreURL.Split(':')[2].Split('@')[0] + ";Database=" + postgreURL.Split(':')[3].Split('/')[1] + "";
    }


    var logHost = builder.Host.UseSerilog((hostingContext, loggerConfiguration) => {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
                          builder =>
                          {
                              builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                          });
    });

    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();


    // configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    


    // configure DI for application services
    services.AddScoped<ITriggerService, TriggerService>();
    services.AddScoped<ITriggerDataContext, TriggerDataContext>();
    services.AddScoped<IPollingService, PollingService>();
    services.AddScoped<IPollingDataContext, PollingDataContext>();

}


var app = builder.Build();

{
    // global cors policy
    //app.UseCors(x => x
    //    .AllowAnyOrigin()
    //    .AllowAnyMethod()
    //    .AllowAnyHeader());


    app.UseCors(MyAllowSpecificOrigins);


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // custom jwt auth middleware
    //app.UseMiddleware<JwtMiddleware>();

    app.MapControllers();

    app.MapGet("/", () => "API running successfully");

    app.UseExceptionHandler(config =>
    {
        config.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var error = context.Features.Get<IExceptionHandlerFeature>();
            if (error != null)
            {
                await context.Response.WriteAsync("An error occurred " + error.Error);
            }
        });
    });

    #region Method1
    //Code for restart Heroku app to keep alive
    // var cancellationTokenSource = new CancellationTokenSource();
    // var token = cancellationTokenSource.Token;
    // ILogger<RestartHerokuClass> logger = app.Services.GetRequiredService<ILogger<RestartHerokuClass>>();
    // IConfiguration config = builder.Configuration.GetSection("AppSettings");
    // RestartHerokuClass herokuObj = new RestartHerokuClass(config, logger);
    // Environment.SetEnvironmentVariable("LastHerokuRestart", DateTime.Now.ToString());
    // string szLastRun = Environment.GetEnvironmentVariable("LastHerokuRestart");
    // DateTime lastRun = DateTime.Now;
    // if (szLastRun != null && szLastRun != "")
    //     DateTime.TryParse(szLastRun, out lastRun);

    // Environment.SetEnvironmentVariable("LastHerokuRestart", lastRun.ToString());
    // int HerokuPollDelay = 30000;

    // Task listener = Task.Factory.StartNew(() =>
    // {
    //     while (true)
    //     {
    //         if (DateTime.Now >= lastRun.AddMinutes(1))
    //         {
    //             int pollStatus = herokuObj.RestartHeroku();
    //             Environment.SetEnvironmentVariable("LastHerokuRestart", DateTime.Now.ToString());

    //             if (pollStatus < 0)
    //                 break;
    //             int.TryParse(config["HerokuPollingDelay"], out HerokuPollDelay);

    //             logger.LogError($"HerokuPollingDelay {HerokuPollDelay}");
    //             if (token.IsCancellationRequested)
    //                 break;
    //         }
    //         Thread.Sleep(HerokuPollDelay);
    //     }

    // } , token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    // listener?.ContinueWith(t => herokuObj.RestartShutDown(t));
    // PollingSingleton.pollingListenerHeroku = listener;
    #endregion


    #region MyRegion
    //Code for restart Heroku app to keep alive
    var cancellationTokenSource = new CancellationTokenSource();
    var token = cancellationTokenSource.Token;
    ILogger<RestartHerokuClass> logger = app.Services.GetRequiredService<ILogger<RestartHerokuClass>>();
    IConfiguration config = builder.Configuration.GetSection("AppSettings");
    RestartHerokuClass herokuObj = new RestartHerokuClass(config, logger);

    Task listener = Task.Factory.StartNew(() =>
    {
        Thread.Sleep(5000);
        int pollStatus = herokuObj.RestartHeroku();
        cancellationTokenSource.Cancel();

    }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    listener?.ContinueWith(t => herokuObj.RestartShutDown(t));
    PollingSingleton.pollingListenerHeroku = listener;
    #endregion

}

app.Run();





