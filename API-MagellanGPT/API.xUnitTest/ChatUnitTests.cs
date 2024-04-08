using System.Net;
using API.Application.Common.Dto;
using API.Application.Common.Interfaces;
using API.Application.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Moq.Protected;

namespace API.xUnitTest;

public class ChatUnitTests
{
    [Fact]
    public async Task Chat_ReturnsExpectedMessages()
    {
        var chatServiceMock = new Mock<IOpenAIService>();
        var userContent = new RequestDto { Model = "gpt3", RequestMessage = "comment vas-tu ?" };
        var kernel = new Kernel();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockMemoryServerless = new Mock<MemoryServerless>();
        var mockChatCompletionService = new Mock<IChatCompletionService>();
        
        // kernel.Setup(k => k.Services.GetRequiredKeyedService<IChatCompletionService>(It.IsAny<string>()))
        //     .Returns(mockChatCompletionService.Object);
        
        var service = new OpenAIService(kernel, mockConfiguration.Object, mockMemoryServerless.Object);

        var response = await service.Chat(userContent).ToListAsync();
        
        Assert.NotEmpty(response);
    }
    
    [Fact]
    public async Task Chat_StatusSuccessfull()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Your expected response string"),
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // Prepare the expected response of the mocked http call
            .ReturnsAsync(response)
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7106/api/OpenAI/Chat"),
        };

        var result = await httpClient.GetAsync(httpClient.BaseAddress);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}