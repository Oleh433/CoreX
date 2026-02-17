using CoreX.Application.DTO;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IUserService
    {
        Task UserRegisterAsync(UserRegisterRequest userRegisterRequest);

        Task AdminRegisterAsync(UserRegisterRequest userRegisterRequest);

        Task TrainerRegisterAsync(UserRegisterRequest userRegisterRequest);

        Task SignInAsync(UserSignInRequest userSignInRequest);

        Task SignOutAsync();
    }
}
