using Domain.API;

namespace Domain.Controller.Public.CandidacyType
{
    public class GetPaginationResponse
    {
        public class Response : PaginationResponse<Data> { }

        public class Data
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
            public required short Position { get; set; }
        }
    }
}
