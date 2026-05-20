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
            var ownerEmail = _configuration["Owner:Email"]?.Trim();
            var ownerPassword = _configuration["Owner:Password"]?.Trim();

            if (string.IsNullOrWhiteSpace(ownerEmail) || string.IsNullOrWhiteSpace(ownerPassword))
            {
                throw new InvalidOperationException(
                    "Owner seeding requires both 'Owner:Email' and 'Owner:Password' configuration values to be set.");
            }

            var existing = await _userManager.FindByEmailAsync(ownerEmail);

            if (existing != null)
            {
                if (!await _userManager.IsInRoleAsync(existing, RoleOptions.Owner.ToString()))
                {
                    await _userManager.AddToRoleAsync(existing, RoleOptions.Owner.ToString());
                }

                return;
            }

            ApplicationUser user = new()
            {
                Email = ownerEmail,
                UserName = ownerEmail,
                FullName = "System Owner"
            };

            var createResult = await _userManager.CreateAsync(user, ownerPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to seed owner account: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, RoleOptions.Owner.ToString());
        }
    }
}
