using Domain.API;

namespace Domain.Controller.Private.ScrutinySlate
{
    public class GetPaginationResponse
    {
        public class Response : PaginationResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required short Position { get; set; }
            public required int CountCandidacies { get; set; }
            public FirstCandidacyData? FirstCandidacy { get; set; }
        }

        public class FirstCandidacyData
        {

            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required Guid SlateId { get; set; }
            public required short CandidacyTypeId { get; set; }
            public required string Name { get; set; }
            public required string LastName { get; set; }
        }
    }
}
