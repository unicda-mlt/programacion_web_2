using Domain.API;

namespace Domain.Controller.Private.Scrutiny
{
    public class GetByIdResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required short StatusId { get; set; }
            public required StatusData Status { get; set; }
            public required string Title { get; set; }
            public required string Description { get; set; }
            public required DateTime StartDate { get; set; }
            public required DateTime EndDate { get; set; }
            public string? ImageUrl { get; set; }
            public string? SignFileUrl { get; set; }
            public required DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class StatusData
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
        }
    }
}
