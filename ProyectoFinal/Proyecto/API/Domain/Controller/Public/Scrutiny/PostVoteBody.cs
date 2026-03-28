using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class PostVoteBody
    {
        public required Guid SlateId { get; set; }
    }
}
