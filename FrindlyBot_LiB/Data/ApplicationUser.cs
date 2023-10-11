using Microsoft.AspNetCore.Identity;

namespace FrindlyBot_LiB.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
