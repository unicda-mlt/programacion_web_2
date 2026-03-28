using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class GetByIdResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public required string Title { get; set; }
            public required string Description { get; set; }
            public required DateTime StartDate { get; set; }
            public required DateTime EndDate { get; set; }
            public string? ImageUrl { get; set; }

            public required SlateData[] Slates { get; set; } = [];
        }

        public class SlateData
        {
            public required Guid Id { get; set; }
            public required Guid ScrutinyId { get; set; }
            public required short Position { get; set; }

            public required CandidacyData[] Candidacies { get; set; } = [];
        }

        public class  CandidacyData
        {
            public required Guid SlateId { get; set; }
            public required short CandidacyTypeId { get; set; }
            public required string Name { get; set; }
            public required string LastName { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}
