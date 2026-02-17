using CoreX.Application.DTO;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain.Enums;
using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace CoreX.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task UserRegisterAsync(UserRegisterRequest userRegisterRequest) =>
            await RegisterAsync(userRegisterRequest, RoleOptions.User);

        public async Task AdminRegisterAsync(UserRegisterRequest userRegisterRequest) =>
            await RegisterAsync(userRegisterRequest, RoleOptions.Admin);

        public async Task TrainerRegisterAsync(UserRegisterRequest userRegisterRequest) =>
            await RegisterAsync(userRegisterRequest, RoleOptions.Trainer);

        public async Task SignInAsync(UserSignInRequest userSignInRequest)
        {
            SignInResult signInResult = await _signInManager.PasswordSignInAsync(
                userSignInRequest.Email,
                userSignInRequest.Password,
                false,
                false);

            if (signInResult.IsLockedOut)
                throw new Exception("User is locked out");

            if (signInResult.IsNotAllowed)
                throw new Exception("User is not allowed");

            if (!signInResult.Succeeded)
                throw new Exception("Invalid email or password");
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        

        private async Task RegisterAsync(UserRegisterRequest userRegisterRequest, RoleOptions roleOption)
        {
            var email = userRegisterRequest.Email.Trim().ToLower();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            ApplicationUser applicationUser = new ApplicationUser()
            {
                FullName = userRegisterRequest.FullName,
                UserName = email,
                Email = email
            };

            IdentityResult identityResult = await _userManager
                .CreateAsync(applicationUser, userRegisterRequest.Password);

            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ",
                   identityResult.Errors.Select(error => error.Description)));
            }

            IdentityResult roleApplyingResult = await _userManager.AddToRoleAsync(applicationUser, roleOption.ToString());

            if (!roleApplyingResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ",
                    roleApplyingResult.Errors.Select(error => error.Description)));
            }
        }
    }
}
