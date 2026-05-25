using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Models
{
    public class RoleViewAccess
    {
        public int UserRoleId { get; set; }
        public int ViewId { get; set; }

        // Навигация
        public virtual UserRole Role { get; set; } = null!;
        public virtual AppView View { get; set; } = null!;
    }
}
