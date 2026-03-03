
namespace Domain.Controller.Private.Auth
{
    public class GetUserInfoResponse
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public required Role UserRole { get; set; }

        public class Role
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
        }
    }
}
