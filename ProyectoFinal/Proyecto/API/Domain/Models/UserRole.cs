
namespace Domain.Models
{
    public class UserRole: BaseEntity<short>
    {
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        
        public ICollection<User> Users { get; set; } = [];
    }

    public enum EUserRole : short
    {
        ADMIN = 1,
        STUDENT = 2
    }

    public static class EUserRoleExtensions
    {
        public static short GetValue(this EUserRole tipoFactura)
        {
            return (short)tipoFactura;
        }

        public static string GetName(this EUserRole tipoFactura)
        {
            return tipoFactura.ToString().ToUpperInvariant();
        }

        public static EUserRole? FromValue(short value)
        {
            return Enum.IsDefined(typeof(EUserRole), value)
                ? (EUserRole)value
                : null;
        }

        public static List<EnumModel<short>> GetList()
        {
            return [.. Enum.GetValues(typeof(EUserRole))
                .Cast<EUserRole>()
                .Select(e => new EnumModel<short>
                {
                    Id = (short)e,
                    Nombre = e.GetName()
                })];
        }
    }
}
