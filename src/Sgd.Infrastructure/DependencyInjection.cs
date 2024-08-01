using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Sgd.Application.Common.Interfaces;
using Sgd.Infrastructure.Persistence;
using Sgd.Infrastructure.Persistence.Configurations;

namespace Sgd.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        return builder
            .AddConfigurations()
            .AddCache()
            .AddMediatR()
            .AddBackgroundServices()
            .AddPersistence()
            .AddServices();
    }

    private static WebApplicationBuilder AddConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();

        return builder;
    }

    private static WebApplicationBuilder AddCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        return builder;
    }

    private static WebApplicationBuilder AddMediatR(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(options =>
            options.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection))
        );

        return builder;
    }

    private static WebApplicationBuilder AddBackgroundServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SgdDbOptions>(
            builder.Configuration.GetSection("SgdDatabase")
        );

        SgdDbModelConfiguration.RegisterSmartEnumSerializers();
        SgdDbModelConfiguration.ConfigureModel();

        builder.Services.AddSingleton<IMongoClient, MongoClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<SgdDbOptions>>()
                .Value;
            return new MongoClient(settings.ConnectionString);
        });

        builder.Services.AddScoped<SgdDbContext>();
        builder.Services.AddScoped<IUnitOfWork, SgdDbContext>();

        return builder;
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
