using Microsoft.SemanticKernel;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;

namespace API.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Charge les param�tres � partir du fichier "appsettings.json"
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var TenantId = builder.Configuration["keyVault:TenantId"];
            var ClientId = builder.Configuration["keyVault:ClientId"];
            var ClientSecret = builder.Configuration["keyVault:ClientSecret"];

            // R�cup�ration des secrets du key vault
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://kv-magellan.vault.azure.net/"),
                new ClientSecretCredential(TenantId, ClientId, ClientSecret)
            );
            
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // récupération des variables d'environnements
            var gpt3Name = builder.Configuration["GPTVersion35"];
            var gpt4Name = builder.Configuration["GPTVersion4"];
            var gptTextEmbeddingName = builder.Configuration["GPTTextEmbedding"];
            var endpoint = builder.Configuration["endpointOpenIA"];
            var keyDeployment = builder.Configuration["DeploymentKeyOpenIA"];

            builder.Services.AddKernel()
                .AddAzureOpenAIChatCompletion(gpt3Name, endpoint, keyDeployment, serviceId: "gpt3")
                .AddAzureOpenAIChatCompletion(gpt4Name, endpoint, keyDeployment, serviceId: "gpt4")
                .AddAzureOpenAIChatCompletion(gptTextEmbeddingName, endpoint, keyDeployment, serviceId: "text-embedding");

            builder.Services.AddTransient<MemoryServerless>(_ => new KernelMemoryBuilder()
                .WithAzureOpenAITextGeneration(
                    new AzureOpenAIConfig
                    {
                        APIKey = builder.Configuration["ApiKeyOpenIA"]!,
                        Endpoint = builder.Configuration["EndpointOpenIA"]!,
                        Deployment = builder.Configuration["GPTVersion35"]!,
                        Auth = AzureOpenAIConfig.AuthTypes.APIKey
                    }
                ).WithAzureOpenAITextEmbeddingGeneration(
                    new AzureOpenAIConfig
                    {
                        APIKey = builder.Configuration["ApiKeyOpenIA"]!,
                        Endpoint = builder.Configuration["EndpointOpenIA"]!,
                        Deployment = builder.Configuration["GPTTextEmbedding"]!,
                        Auth = AzureOpenAIConfig.AuthTypes.APIKey
                    }
                ).Build<MemoryServerless>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
