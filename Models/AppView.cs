using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Models
{
    // Да, имя чутка выбивающееся, но оно тут в значении регулировка доступа как раз к самим вкладкам - представлениям
    public class AppView
    {
        public int ViewId { get; set; }
        public string ViewCode { get; set; } = null!;
        public string ViewName { get; set; } = null!;

        // Навигация
        public virtual ICollection<RoleViewAccess> RoleViewAccesses { get; set; } = new List<RoleViewAccess>();
    }
}
