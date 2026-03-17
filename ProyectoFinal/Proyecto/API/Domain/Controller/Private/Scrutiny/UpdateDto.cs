
namespace Domain.Controller.Private.Scrutiny
{
    public class UpdateDto
    {
        public short? StatusId { get; set; } = null;
        public string? Title { get; set; } = null;
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
    }
}
