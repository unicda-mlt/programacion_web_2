using Business.Authentication;
using Business.Utils;
using Data.Repositories;
using Domain.Controller.Private.Auth;

namespace Business.Controllers
{
    public class AuthService(AuthenticationService authService, UsuarioRepository usuarioRepository)
    {
        private readonly AuthenticationService _authService = authService;
        private readonly UsuarioRepository _usuarioRepository = usuarioRepository;

        public async Task<string?> GenerateAuthUserToken (GenerateUserTokenDto data)
        {
            if (string.IsNullOrEmpty(data.UserName) || string.IsNullOrEmpty(data.Password))
            {
                return null;
            }

            var usuario = await _usuarioRepository.GetOneByFilter(x => x.UserName == data.UserName);

            if (usuario == null) {
                return null;
            }

            var isCorrectPassword = PasswordHasher.VerifyPassword(data.Password, usuario.Password);

            if (!isCorrectPassword)
            {
                return null;
            }

            return _authService.GenerateUserToken(usuario.Id, usuario.UserRoleId);
        }

        public async Task<GetUserInfoResponse?> GetUserInfoResponse(string token)
        {
            var tokenInfo = _authService.GetTokenUserInfo(token);

            if (tokenInfo == null) { 
                return null;
            }

            var usuario = await _usuarioRepository.GetById(tokenInfo.UserId, "UserRole");

            if (usuario == null) {
                return null;
            }

            return new()
            {
                UserRole = new()
                {
                    Id = usuario.UserRole.Id,
                    Name = usuario.UserRole.Name
                },
                UserName = usuario.UserName
            };
        }
    }
}
