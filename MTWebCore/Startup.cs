using Lamar;
using MassTransit;

namespace MTWebCore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void ConfigureContainer(ServiceRegistry services)
    {
        var serviceBusConnectionString = _configuration["ServiceBusConnectionString"];
        var accountsServiceBusConnectionString = _configuration["AccountsServiceBusConnectionString"];
        
        services.AddControllers();
        
        services.AddMassTransit(cfg =>
        {
              cfg.UsingAzureServiceBus((context, sbCfg) =>
                {
                    var connectionString = serviceBusConnectionString;
                    sbCfg.Host(connectionString);
                    sbCfg.UseServiceBusMessageScheduler();

                    sbCfg.ConfigureEndpoints(context);
                });
            });

        services.AddMassTransit<IAccountsBus>(cfg =>
        {
            cfg.AddRequestClient<GetPlanFeaturesRequest>();

            cfg.UsingAzureServiceBus((context, sbCfg) =>
            {
                sbCfg.Host(accountsServiceBusConnectionString);
                sbCfg.UseServiceBusMessageScheduler();


                sbCfg.ConfigureEndpoints(context);
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}