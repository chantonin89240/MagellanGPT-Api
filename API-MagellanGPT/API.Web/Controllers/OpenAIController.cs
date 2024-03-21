using API.Application.Common.Dto;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.KernelMemory;
using System.IO;
using System.Reflection.Metadata;
using Azure.Search.Documents;
using Azure;
using Microsoft.KernelMemory.Models;

namespace API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private Kernel kernel;
        private IConfiguration configuration;
        private MemoryServerless memoryServerless;

        public OpenAIController(Kernel kernel, IConfiguration configuration, MemoryServerless serverless)
        {
            this.kernel = kernel;
            this.configuration = configuration;
            this.memoryServerless = serverless;
        }

        [HttpPost("Chat")]
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

        [HttpPost("ChatWithPromptSystem")]
        public async IAsyncEnumerable<string> ChatWithPromptSystem([FromBody] MessageByRoleDto userMessage)
        {
            var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(userMessage.Model);

            var chatHistory = new ChatHistory();

            if (userMessage.Messages != null && userMessage.Messages.Any())
            {
                foreach (var message in userMessage.Messages)
                {
                    if (message.Role == "System" || message.Role == "system")
                    {
                        chatHistory.AddSystemMessage(message.Content);
                    }
                    else if (message.Role == "User" || message.Role == "user")
                    {
                        chatHistory.AddUserMessage(message.Content);
                    }
                }
            }

            await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                yield return item.Content;
            }
        }

        /// <summary>
        /// fonctionnalité de RAG pour questionner l'AI sur des documents PDF
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        [HttpPost("RAG")]
        public async IAsyncEnumerable<string> Rag([FromForm] RagDto userMessage)
        {
            // vérification que la liste contient des document 
            if (userMessage.Files == null || !userMessage.Files.Any())
            {
                yield return "No files were provided.";
                yield break;
            }

            List<string> idDoc = new List<string>();
            // lecture des documents avec un stream et ajout dans le serverless 
            foreach (var file in userMessage.Files)
            {
                // vérification que tout les documents soit des PDF
                if (file.ContentType != "application/pdf")
                {
                    yield return "One document is not a PDF.";
                    yield break;
                }

                var myDoc = new Microsoft.KernelMemory.Document();

                myDoc.AddStream(file.FileName, file.OpenReadStream());
                var id = await memoryServerless.ImportDocumentAsync(myDoc);
                idDoc.Add(id);
            }

            // ajout de la question utilisateur
            var answer = await memoryServerless.AskAsync(userMessage.RequestMessage);

            // return de la réponse de l'AI
            yield return answer.Result;

            // on vide les documents pour éviter les erreurs de cache
            idDoc.ForEach(id =>
            {
                memoryServerless.DeleteDocumentAsync(id);
            });
        }

        /// <summary>
        /// fonctionnalité d'upload des documents dans azure storage avec un Blob
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<string> UploadFileToBlobAsync(IFormFile file)
        {
            var connectionString = configuration["storage:connectionString"];
            var containerName = configuration["storage:containerName"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }
    }
}
