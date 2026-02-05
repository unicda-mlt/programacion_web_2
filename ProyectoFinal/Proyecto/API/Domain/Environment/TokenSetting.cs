
namespace Domain.Environment
{
    public class TokenSetting
    {
        public required string Issuer { get; set; }
        public required User UserScheme { get; set; }

        public class User
        {
            public required string Audience { get; set; }
            public required int ExpiresInMinutes { get; set; }
            public required string Key { get; set; }

        }
    }
}
