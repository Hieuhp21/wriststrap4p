using WEB_SHOW_WRIST_STRAP.Data;
using WEB_SHOW_WRIST_STRAP.Core.Repositories;

namespace WEB_SHOW_WRIST_STRAP.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserLoginContext _context;

        public UserRepository(UserLoginContext context)
        {
            _context = context;
        }

        public ICollection<ApplicationUser> GetUsers()
        {
            return _context.Users.ToList();
        }

        public ApplicationUser GetUser(string id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public ApplicationUser UpdateUser(ApplicationUser user)
        {
             _context.Update(user);
             _context.SaveChanges();

             return user;
        }

        public bool DeleteUser(string id)
        {
            try
            {
                _context.Users.Remove(_context.Users.FirstOrDefault(u => u.Id == id));
                _context.SaveChanges();
            }
            catch { return false; }
            return true;
        }

        public bool DeleteUser(ApplicationUser user)
        {
            try
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            catch { return false; }
            return true;
        }
        //public bool DelRole(string userId)
        //{
        //    try
        //    {
        //        _context.Users.Remove(_context.Users.FirstOrDefault(u => u.Id == id));
        //        _context.SaveChanges();
        //    }
        //    catch { return false; }
        //    return true;
        //}
    }
}
