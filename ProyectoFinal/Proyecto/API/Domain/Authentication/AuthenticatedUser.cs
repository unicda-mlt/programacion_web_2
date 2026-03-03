
namespace Domain.Authentication
{
    public class AuthenticatedUser
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public required Role UserRole { get; set; }
        public required TokenUserInfo TokenInfo { get; set; }
        public required bool Active { get; set; }

        public class Role
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
        }
    }
}
