using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerZdec.Services
{
    public interface IPermissionService
    {
        bool HasAccess(string viewCode, int userRoleId);

        Task LoadPermissionsAsync();
    }
}
