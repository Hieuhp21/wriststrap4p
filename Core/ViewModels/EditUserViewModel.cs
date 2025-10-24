using WEB_SHOW_WRIST_STRAP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WEB_SHOW_WRIST_STRAP.Core.ViewModels
{
    public class EditUserViewModel
    {
        public ApplicationUser User { get; set; }

        public string NewPassword { get; set; }

        public IList<SelectListItem> Roles { get; set; }
    }
}
