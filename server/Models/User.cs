using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaletteItem> PaletteItems { get; set; } = new List<PaletteItem>();
        //public ICollection<Scheme> Schemes { get; set; } = new List<Scheme>();

        public User()
        {
            
        }
    }
}