
namespace Domain.Models
{
    public class Scrutiny : BaseEntity<Guid>
    {
        public short StatusId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }

        public ScrutinyStatus ScrutinyStatus { get; set; } = default!;
        public ICollection<ScrutinySign> ScrutinySigns { get; set; } = [];
        public ICollection<Slate> Slates { get; set; } = [];
        public ICollection<Vote> Votes { get; set; } = [];
    }
}
