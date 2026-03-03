
namespace Domain.Models
{
    public class Vote : BaseEntity<Guid>
    {
        public Guid ScrutinyId { get; set; }
        public Guid SlateId { get; set; }
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime IssueDate { get; set; }

        public Scrutiny Scrutiny { get; set; } = default!;
        public Slate Slate { get; set; } = default!;
        public User User { get; set; } = default!;
        public Student Student { get; set; } = default!;
    }
}
