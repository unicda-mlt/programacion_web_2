
namespace Domain.Models
{
    public class User: BaseEntity<Guid>
    {
        public short UserRoleId { get; set; }
        public Guid? StudentId { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool Active { get; set; } = false;

        public UserRole UserRole { get; set; } = default!;
        public Student? Student { get; set; }
    }
}
