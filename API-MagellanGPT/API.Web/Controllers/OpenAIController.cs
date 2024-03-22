using API.Application.Common.Dto;
using API.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private Kernel kernel;
        private IOpenAIService OpenAIService;
        private IConfiguration configuration;
        private MemoryServerless memoryServerless;

        public OpenAIController(Kernel kernel, IOpenAIService openAIService, IConfiguration configuration, MemoryServerless serverless)
        {
            this.kernel = kernel;
            this.OpenAIService = openAIService;
            this.configuration = configuration;
            this.memoryServerless = serverless;
        }

        /// <summary>
        /// fonctionnalité de chat avec azure open AI
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        [HttpPost("Chat")]
        public async IAsyncEnumerable<string> Chat([FromBody] RequestDto userMessage) 
        {
            // appel du service
            await foreach (var message in OpenAIService.Chat(userMessage))
            {
                yield return message;
            }
        }

        /// <summary>
        /// fonctionnalité de chat avec les paramètres de prompt system
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        [HttpPost("ChatWithPromptSystem")]
        public async IAsyncEnumerable<string> ChatWithPromptSystem([FromBody] MessageByRoleDto userMessage)
        {
            // appel du service
            await foreach (var message in OpenAIService.ChatWithPromptSystem(userMessage))
            {
                yield return message;
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
            // appel du service 
            await foreach (var message in OpenAIService.Rag(userMessage))
            {
                yield return message;
            }
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
