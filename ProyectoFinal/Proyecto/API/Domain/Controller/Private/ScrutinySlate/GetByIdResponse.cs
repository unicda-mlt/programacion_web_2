using Domain.API;

namespace Domain.Controller.Private.ScrutinySlate
{
    public class GetByIdResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required short Position { get; set; }

            public required List<CandidacyData> Candidacies { get; set; }
        }

        public class CandidacyData
        {
            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required Guid SlateId { get; set; }
            public required short CandidacyTypeId { get; set; }
            public required string Name { get; set; }
            public required string LastName { get; set; }
            public string? ImageUrl { get; set; }

            public required CandidacyTypeData CandidacyType { get; set; }
        }

        public class CandidacyTypeData
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
        }
    }
}
