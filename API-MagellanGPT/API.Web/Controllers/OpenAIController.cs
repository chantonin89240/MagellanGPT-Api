using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace API.Web.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private Kernel kernel;
        
        public OpenAIController(Kernel kernel)
        {
            this.kernel = kernel;
        }

        [HttpGet]
        public async IAsyncEnumerable<string> Chat(string userMessage) 
        {
            var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>("gpt3");

            var chatHistory = new ChatHistory();

            chatHistory.AddUserMessage(userMessage);

            await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                yield return item.Content;
            }
        }
    }
}
