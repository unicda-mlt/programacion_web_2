
namespace Domain.API
{
    public class BadRequestResponse
    {
        public bool Ok { get; } = false;
        public required string BadMessage { get; set; }
    }
}
