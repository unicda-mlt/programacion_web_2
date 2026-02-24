using Business.Authentication;
using Business.Utils;
using Data.Repositories;
using Domain.Authentication;
using Domain.Controller.Private.Auth;

namespace Business.Controllers
{
    public class AuthService(AuthenticationService authService, UserRepository userRepository)
    {
        private readonly AuthenticationService _authService = authService;
        private readonly UserRepository _userRepository = userRepository;

        public async Task<string?> GenerateAuthUserToken (GenerateUserTokenDto data)
        {
            if (string.IsNullOrEmpty(data.UserName) || string.IsNullOrEmpty(data.Password))
            {
                return null;
            }

            var user = await _userRepository.GetOneByFilter(x => x.UserName == data.UserName);

            if (user == null || !user.Active) {
                return null;
            }

            var isCorrectPassword = PasswordHasher.VerifyPassword(data.Password, user.Password);

            if (!isCorrectPassword)
            {
                return null;
            }

            return _authService.GenerateUserToken(user.Id, user.UserRoleId);
        }

        public async Task<AuthenticatedUser?> GetUser(string token)
        {
            var tokenInfo = _authService.GetTokenUserInfo(token);

            if (tokenInfo == null) { 
                return null;
            }

            var user = await _userRepository.GetById(tokenInfo.UserId, "UserRole");

            if (user == null) {
                return null;
            }

            return new()
            {
                UserRole = new()
                {
                    Id = user.UserRole.Id,
                    Name = user.UserRole.Name
                },
                UserName = user.UserName,
                TokenInfo = tokenInfo,
                Active = user.Active
            };
        }
    }
}
