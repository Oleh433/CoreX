using CoreX.Domain.Enums;
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CoreX.Infrastructure.Identity
{
    public class IdentityInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public IdentityInitializer(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task CreateRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync(RoleOptions.User.ToString()))
            {
                await _roleManager.CreateAsync(new ApplicationRole(RoleOptions.User.ToString()));
            }

            if (!await _roleManager.RoleExistsAsync(RoleOptions.Trainer.ToString()))
            {
                await _roleManager.CreateAsync(new ApplicationRole(RoleOptions.Trainer.ToString()));
            }

            if (!await _roleManager.RoleExistsAsync(RoleOptions.Admin.ToString()))
            {
                await _roleManager.CreateAsync(new ApplicationRole(RoleOptions.Admin.ToString()));
            }

            if (!await _roleManager.RoleExistsAsync(RoleOptions.Owner.ToString()))
            {
                await _roleManager.CreateAsync(new ApplicationRole(RoleOptions.Owner.ToString()));
            }
        }

        public async Task AddOwnerAsync()
        {
            IEnumerable<ApplicationUser> owners = await _userManager.GetUsersInRoleAsync(RoleOptions.Owner.ToString());

            if (owners.Count() == 0)
            {
                var ownerEmail = _configuration["Owner:Email"]?.Trim();
                var ownerPassword = _configuration["Owner:Password"]?.Trim();

                ApplicationUser user = new()
                {
                    Email = ownerEmail,
                    UserName = ownerEmail,
                    FullName = "System Owner"
                };

                await _userManager.CreateAsync(user, ownerPassword);

                await _userManager.AddToRoleAsync(user, RoleOptions.Owner.ToString());
            }
        }
    }
}
