
namespace Domain.Controller.Private.Scrutiny
{
    public class AddDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }
}
