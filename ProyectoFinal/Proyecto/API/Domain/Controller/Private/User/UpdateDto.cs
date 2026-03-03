
namespace Domain.Controller.Private.User
{
    public class UpdateDto
    {
        public short? UserRoleId { get; set; } = null;
        public Guid? StudentId { get; set; } = null;
        public string? UserName { get; set; } = null;
        public string? Password { get; set; } = null;
        public bool? Active { get; set; } = null;
    }
}
