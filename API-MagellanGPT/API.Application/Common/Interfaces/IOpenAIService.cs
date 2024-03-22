using API.Application.Common.Dto;

namespace API.Application.Common.Interfaces;

public interface IOpenAIService
{
    // fonction de chat 
    IAsyncEnumerable<string> Chat(RequestDto userMessage);
    // fonction de chat avec prompt system
    IAsyncEnumerable<string> ChatWithPromptSystem(MessageByRoleDto userMessage);
    // fonction de chat avec RAG
    IAsyncEnumerable<string> Rag(RagDto userMessage);
}
