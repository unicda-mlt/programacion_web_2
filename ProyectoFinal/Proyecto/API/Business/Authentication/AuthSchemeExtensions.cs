using Domain.Authentication;

namespace Business.Authentication
{
    public static class AuthSchemeExtensions
    {
        public static string ToSchemeName(this AuthScheme scheme) => scheme switch
        {
            AuthScheme.User => "UserScheme",
            _ => throw new ArgumentOutOfRangeException(nameof(scheme), $"Unknown scheme: {scheme}")
        };

        public static string ToSwaggerId(this AuthScheme scheme) => scheme switch
        {
            AuthScheme.User => "UserBearer",
            _ => throw new ArgumentOutOfRangeException(nameof(scheme), $"Unknown Swagger ID: {scheme}")
        };
    }
}
