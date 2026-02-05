
namespace Domain.Models
{
    public class Student : BaseEntity<Guid>
    {
        public string RegistrationNumber { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool Graduated { get; set; } = false;

        public ICollection<User> Users { get; set; } = [];
    }
}
