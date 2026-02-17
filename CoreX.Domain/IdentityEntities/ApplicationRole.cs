using Microsoft.AspNetCore.Identity;

namespace CoreX.Domain.IdentityEntities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() { }

        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
