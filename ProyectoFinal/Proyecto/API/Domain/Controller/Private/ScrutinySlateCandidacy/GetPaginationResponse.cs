using Domain.API;

namespace Domain.Controller.Private.ScrutinySlateCandidacy
{
    public class GetPaginationResponse
    {
        public class Response : PaginationResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required Guid SlateId { get; set; }
            public required short CandidacyTypeId { get; set; }
            public required string Name { get; set; }
            public required string LastName { get; set; }
            public string? ImageUrl { get; set; }
            public required short CandidacyTypePosition { get; set; }
        }
    }
}
