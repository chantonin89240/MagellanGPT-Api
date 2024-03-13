
using Microsoft.SemanticKernel;

namespace API.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddKernel()
                .AddAzureOpenAIChatCompletion("gpt-35", "https://magellan-gpt.openai.azure.com/", "10306409e1964dd994d7be5a04daf638", serviceId:"gpt3")
                .AddAzureOpenAIChatCompletion("gpt-4", "https://magellan-gpt.openai.azure.com/", "10306409e1964dd994d7be5a04daf638", serviceId:"gpt4");

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
