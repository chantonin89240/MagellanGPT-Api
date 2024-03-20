namespace API.Application.Common.Dto
{
    public class MessageByRoleDto
    {
        public string? Model { get; set; }
        public List<MessageDto>? Messages { get; set; }
    }
}
