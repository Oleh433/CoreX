namespace CoreX.Application.DTO
{
    public class UserRegisterRequest
    {
        public required string FullName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
