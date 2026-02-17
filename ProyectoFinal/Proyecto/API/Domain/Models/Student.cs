
namespace Domain.Models
{
    public class Student : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string RegistrationNumber { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool Graduated { get; set; } = false;

        public User User { get; set; } = default!;
    }
}
