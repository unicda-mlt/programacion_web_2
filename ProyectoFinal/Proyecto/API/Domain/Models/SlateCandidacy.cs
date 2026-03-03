
namespace Domain.Models
{
    public class SlateCandidacy : BaseEntity<Guid>
    {
        public Guid SlateId { get; set; }
        public short CandidacyTypeId { get; set; }
        public string Name { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? ImageUrl { get; set; } = default!;

        public Slate Slate { get; set; } = default!;
    }
}
