
namespace Domain.Authentication
{
    public interface IAuthenticationService
    {
        public TokenUserInfo? GetTokenUserInfo(string token);
        string GenerateUserToken(Guid userId, short userRoleId);
    }
}
