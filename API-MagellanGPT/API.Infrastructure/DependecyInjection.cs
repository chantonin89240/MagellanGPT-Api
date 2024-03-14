using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Microsoft.Azure.Cosmos;
namespace API.Infrastructure;

public static class DependecyInjection
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddScoped<CosmosClient>;


        //services.AddScoped<ICosmosDbService>(provider =>
        //{
        //    var cosmosDbOptions = provider.GetRequiredService<CosmosDbOptions>();
        //    return new CosmosDbService(cosmosDbOptions);
        //});

        //services.AddScoped<IMyEntityRepository, MyEntityRepository>();

        return services;
    }
}
