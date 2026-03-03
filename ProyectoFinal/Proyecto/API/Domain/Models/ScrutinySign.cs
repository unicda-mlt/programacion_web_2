
namespace Domain.Models
{
    public class ScrutinySign : BaseEntity<Guid>
    {
        public Guid ScrutinyId { get; set; }
        public string FileUrl { get; set; } = default!;

        public Scrutiny Scrutiny { get; set; } = default!;
    }
}
