using WEB_SHOW_WRIST_STRAP.Data;
using Microsoft.AspNetCore.Identity;

namespace WEB_SHOW_WRIST_STRAP.Core.Repositories
{
    public interface IRoleRepository
    {
        ICollection<IdentityRole> GetRoles();
    }
}
