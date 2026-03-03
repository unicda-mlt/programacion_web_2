using Domain.API;

namespace Domain.Controller.Private.User
{
    public class GetPaginationResponse
    {
        public class Response : PaginationResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required short UserRoleId { get; set; }
            public required string UserName { get; set; }
            public required bool Active { get; set; }
        }
    }
}
