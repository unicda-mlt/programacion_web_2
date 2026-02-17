
namespace Domain.Controller.Private.User
{
    public class AddDto
    {
        public required short UserRoleId { get; set; }
        public Guid? StudentId { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public bool Active { get; set; } = false;
    }
}
