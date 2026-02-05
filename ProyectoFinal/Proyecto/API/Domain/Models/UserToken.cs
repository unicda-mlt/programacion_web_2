
namespace Domain.Models
{
    public class UserToken: BaseEntity<Guid>
    {
        public Guid UserId { get; set; } = default!;
        public string Purpose { get; set; } = default!;
        public string Value { get; set; } = default!;
        public DateTime ExpirationTime { get; set; } = default!;

        public User User { get; set; } = default!;
    }
}
