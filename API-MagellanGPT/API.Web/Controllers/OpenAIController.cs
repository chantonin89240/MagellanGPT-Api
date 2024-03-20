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
            if (userMessage.Files == null || !userMessage.Files.Any())
            {
                yield return "No files were provided.";
                yield break;
            }
            foreach (var file in userMessage.Files)
            {
                var toto = new Microsoft.KernelMemory.Document();

                toto.AddStream(file.FileName, file.OpenReadStream());
                //toto.AddFile(file.FileName);
                var id = await memoryServerless.ImportDocumentAsync(toto);
            }

            var answer = await memoryServerless.AskAsync(userMessage.RequestMessage);

            yield return answer.Result;
        }

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
