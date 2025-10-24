using WEB_SHOW_WRIST_STRAP.Data;
using WEB_SHOW_WRIST_STRAP.Core.Repositories;
using Microsoft.AspNetCore.Identity;

namespace WEB_SHOW_WRIST_STRAP.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly UserLoginContext _context;

        public RoleRepository(UserLoginContext context)
        {
            _context = context;
        }

        public ICollection<IdentityRole> GetRoles()
        {
            return _context.Roles.ToList();
        }
    }
}
