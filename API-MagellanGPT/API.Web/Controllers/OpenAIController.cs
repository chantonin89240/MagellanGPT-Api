using API.Application.Common.Dto;
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

        [HttpPost]
        public async IAsyncEnumerable<string> Chat([FromBody] RequestDto userMessage) 
        {
            var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(userMessage.Model);
            //var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>("gpt3");

            var chatHistory = new ChatHistory();

            chatHistory.AddUserMessage(userMessage.RequestMessage);

            await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                yield return item.Content;
            }
        }
    }
}
