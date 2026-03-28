using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class GetPaginationResponse
    {
        public class Response : PaginationResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required string Title { get; set; }
            public required string Description { get; set; }
            public required DateTime StartDate { get; set; }
            public required DateTime EndDate { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}
