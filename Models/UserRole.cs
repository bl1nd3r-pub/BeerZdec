using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Models
{
    public class UserRole
    {
        public int UserRoleId { get; set; }
        public string RoleName { get; set; } = null!;

        // Навигация
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        public virtual ICollection<RoleViewAccess> RoleViewAccesses { get; set; } = new List<RoleViewAccess>();
    }
}
