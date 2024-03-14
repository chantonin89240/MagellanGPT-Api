
using Microsoft.SemanticKernel;
using Azure.Identity;

namespace API.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://kv-magellan.vault.azure.net/"),
                new ClientSecretCredential("1e70e2d5-3cab-43fd-805b-4ffde4d58432", "8765f370-18f1-434c-92ad-be4ad238cbf5", "cu~8Q~PROoaSdQRqMGF4xCmp22fChqkUKYkz-cdh")
            );

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            

            var gpt3Name = builder.Configuration["GPTVersion35"];
            var gpt4Name = builder.Configuration["GPTVersion4"];
            var endpoint = builder.Configuration["endpointOpenIA"];
            var keyDeployment = builder.Configuration["DeploymentKeyOpenIA"];

            builder.Services.AddKernel()
                .AddAzureOpenAIChatCompletion(gpt3Name, endpoint, keyDeployment, serviceId: "gpt3")
                .AddAzureOpenAIChatCompletion(gpt4Name, endpoint, keyDeployment, serviceId: "gpt4");

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
