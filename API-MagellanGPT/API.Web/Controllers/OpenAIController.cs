using API.Application.Common.Dto;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.IO;
using System.Reflection.Metadata;

namespace API.Web.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private Kernel kernel;
        private IConfiguration configuration;
        
        public OpenAIController(Kernel kernel, IConfiguration configuration)
        {
            this.kernel = kernel;
            this.configuration = configuration;
        }

        [HttpPost]
        public async IAsyncEnumerable<string> Chat([FromBody] RequestDto userMessage) 
        {
            var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(userMessage.Model);

            var chatHistory = new ChatHistory(); // a revoir pour l'historique

            chatHistory.AddUserMessage(userMessage.RequestMessage);

            await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                yield return item.Content;
            }
        }

        [HttpPost("RAG")]
        public async IAsyncEnumerable<string> Rag([FromForm] RagDto userMessage)
        {
            // Vérifiez si des fichiers ont été envoyés
            if (userMessage.Files == null || !userMessage.Files.Any())
            {
                yield return "No files were provided.";
                yield break;
            }

            // Create a BlobServiceClient that will authenticate through Active Directory
            Uri accountUri = new Uri(configuration["storage:urlBlob"]);
            BlobServiceClient client = new BlobServiceClient(accountUri, new DefaultAzureCredential());

            BlobContainerClient blobContainer = new BlobContainerClient(configuration["storage:connectionString"], configuration["storage:containerName"]);

            // Récupérez les documents pertinents à partir des fichiers envoyés
            var relevantDocuments = new List<object>();
            foreach (var file in userMessage.Files)
            {
                if (file.Length > int.MaxValue)
                {
                    yield return "File is too large.";
                    yield break;
                }

                var stream = file.OpenReadStream();
                var blobClient = blobContainer.GetBlobClient(file.FileName);
                blobClient.Upload(stream);

            }

            // Ajoutez les documents récupérés à l'historique de la conversation
            var chatHistory = new ChatHistory();
            //chatHistory.AddUserMessage(userMessage.RequestMessage);
            //foreach (var document in relevantDocuments)
            //{
            //    chatHistory.AddSystemMessage(document);
            //}

            // Utilisez le modèle de langage pour générer une réponse en fonction des documents récupérés
            var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(userMessage.Model);
            await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                yield return item.Content;
            }
        }
    }
}
