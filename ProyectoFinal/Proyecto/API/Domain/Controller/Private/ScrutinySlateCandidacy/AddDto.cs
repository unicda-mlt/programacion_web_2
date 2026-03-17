
namespace Domain.Controller.Private.ScrutinySlateCandidacy
{
    public class AddDto
    {
        public required short CandidacyTypeId { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
    }
}
