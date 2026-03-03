using Domain.API;

namespace Domain.Controller.Private.User
{
    public class GetByIdResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required short UserRoleId { get; set; }
            public Guid? StudentId { get; set; }
            public required string UserName { get; set; }
            public required bool Active { get; set; }
            public required DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}
