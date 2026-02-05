
namespace Domain.Controller.Private.Auth
{
    public class GenerateUserTokenDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
