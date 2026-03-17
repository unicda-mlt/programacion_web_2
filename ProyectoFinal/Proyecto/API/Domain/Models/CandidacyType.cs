
namespace Domain.Models
{
    public class CandidacyType : BaseEntity<short>
    {
        public string Name { get; set; } = default!;
        public short Position { get; set; }

        public ICollection<SlateCandidacy> Candidacies { get; set; } = [];
    }
}
