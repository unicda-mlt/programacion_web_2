
namespace Domain.Models
{
    public class Slate : BaseEntity<Guid>
    {
        public Guid ScrutinyId { get; set; }
        public short Position { get; set; }

        public Scrutiny Scrutiny { get; set; } = default!;
        public ICollection<SlateCandidacy> SlateCandidacies { get; set; } = [];
        public ICollection<Vote> Votes { get; set; } = [];
    }
}
