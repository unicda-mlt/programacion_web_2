using Domain.API;

namespace Domain.Controller.Private.CandidacyType
{
    public class GetByIdResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required short Id { get; set; }
            public required string Name { get; set; }
            public required short Position { get; set; }
        }
    }
}
