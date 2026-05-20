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
        private readonly IEmailSender _emailSender;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
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
                isPersistent: false,
                lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
                throw new UnauthorizedAccessException("Account is temporarily locked due to multiple failed sign-in attempts.");

            if (signInResult.IsNotAllowed)
                throw new UnauthorizedAccessException("Account is not allowed to sign in.");

            if (!signInResult.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        private async Task RegisterAsync(UserRegisterRequest userRegisterRequest, RoleOptions roleOption)
        {
            if (!userRegisterRequest.TermsAccepted)
                throw new InvalidOperationException("Terms of use must be accepted.");

            if (userRegisterRequest.Password != userRegisterRequest.ConfirmPassword)
                throw new InvalidOperationException("Password and ConfirmPassword do not match.");

            var email = userRegisterRequest.Email.Trim().ToLower();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
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

            await _emailSender.SendAsync(
                email,
                "Registration successful",
                $"Hello {applicationUser.FullName}, your account has been registered with role '{roleOption}'.");
        }
    }
}
