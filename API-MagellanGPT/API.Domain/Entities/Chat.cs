using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Domain.Entities
{
    public class Chat
    {
        public Guid IdChat { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateChat { get; set; }
    }
}
