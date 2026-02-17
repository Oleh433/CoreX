using Microsoft.AspNetCore.Identity;

namespace CoreX.Domain.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName {  get; set; }
    }
}
