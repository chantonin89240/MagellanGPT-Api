using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Domain.Entities
{
    public class Request
    {
        public int IdRequest { get; set; }
        public Guid IdChat { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set;}
        public int TokenUsage { get; set; }
    }
}
