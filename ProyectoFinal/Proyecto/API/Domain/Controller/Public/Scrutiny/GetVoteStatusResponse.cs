using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class GetVoteStatusResponse
    {
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public bool HasVoted { get; set; }
            public Guid? SlateId { get; set; }
            public DateTime? VoteDate { get; set; }
        }
    }
}
