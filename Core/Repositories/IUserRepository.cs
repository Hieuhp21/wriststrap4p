using WEB_SHOW_WRIST_STRAP.Data;

namespace WEB_SHOW_WRIST_STRAP.Core.Repositories
{
    public interface IUserRepository
    {
        ICollection<ApplicationUser> GetUsers();

        ApplicationUser GetUser(string id);

        ApplicationUser UpdateUser(ApplicationUser user);

        bool DeleteUser(string id);

        bool DeleteUser(ApplicationUser user);

        //bool DelRole(string userId);
    }
}
