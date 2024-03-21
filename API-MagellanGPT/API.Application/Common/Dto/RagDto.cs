using Microsoft.AspNetCore.Http;

namespace API.Application.Common.Dto
{
    public class RagDto
    {
        public string? Model { get; set; }
        public string? RequestMessage { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
