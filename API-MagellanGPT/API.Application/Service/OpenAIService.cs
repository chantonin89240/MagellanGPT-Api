using API.Application.Common.Dto;
using API.Application.Common.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.IO.Source;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Document = Microsoft.KernelMemory.Document;

namespace API.Application.Service;

public class OpenAIService : IOpenAIService
{
    private Kernel kernel;
    private IConfiguration configuration;
    private MemoryServerless memoryServerless;

    public OpenAIService(Kernel kernel, IConfiguration configuration, MemoryServerless serverless)
    {
        this.kernel = kernel;
        this.configuration = configuration;
        this.memoryServerless = serverless;
    }

    public async IAsyncEnumerable<string> Chat(RequestDto userMessage)
    {
        var chat = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(userMessage.Model);

        var chatHistory = new ChatHistory(); // a revoir pour l'historique

        chatHistory.AddUserMessage(userMessage.RequestMessage);

        await foreach (var item in chat.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            yield return item.Content;
        }
    }

    public async IAsyncEnumerable<string> ChatWithPromptSystem(MessageByRoleDto userMessage)
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
    public async IAsyncEnumerable<string> Rag(RagDto userMessage)
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

            // vérification du poid du fichier PDF
            if (file.Length > int.MaxValue)
            {
                yield return "File is too large.";
                yield break;
            }

            var myDoc = new Document();

            myDoc.AddStream(file.FileName, file.OpenReadStream());
            var id = await memoryServerless.ImportDocumentAsync(myDoc);
            idDoc.Add(id);
        }

        // ajout de la question utilisateur
        var answer = await memoryServerless.AskAsync(userMessage.RequestMessage);

        // pour retourner les sources et citations
        foreach (var x in answer.RelevantSources)
        {
            var text = $"  * {x.SourceName} -- {x.Partitions.First().LastUpdate:D}";
        }

        // return de la réponse de l'AI
        yield return answer.Result;

        // on vide les documents pour éviter les erreurs de cache
        idDoc.ForEach(id =>
        {
            memoryServerless.DeleteDocumentAsync(id);
        });
    }
}
