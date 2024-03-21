using API.Application.Common.Interfaces;
using API.Application.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Application;

public static class DependecyInjection
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IOpenAIService, OpenAIService>();
    }
}
