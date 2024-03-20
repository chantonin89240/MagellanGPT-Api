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
                    if (message.Role == "System")
                    {
                        chatHistory.AddSystemMessage(message.Content);
                    }
                    else if (message.Role == "User")
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
    }
}
