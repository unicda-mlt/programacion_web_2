
namespace Domain.Models
{
    public class ScrutinyStatus : BaseEntity<short>
    {
        public string Name { get; set; } = default!;

        public ICollection<Scrutiny> Scrutinies { get; set; } = [];
    }

    public enum EScrutinyStatus : short
    {
        PENDING = 1,
        OPEN = 2,
        CLOSED = 3,
        SIGNED = 4
    }

    public static class EScrutinyStatusExtensions
    {
        public static short GetValue(this EScrutinyStatus tipoFactura)
        {
            return (short)tipoFactura;
        }

        public static string GetName(this EScrutinyStatus tipoFactura)
        {
            return tipoFactura.ToString().ToUpperInvariant();
        }

        public static EScrutinyStatus? FromValue(short value)
        {
            return Enum.IsDefined(typeof(EScrutinyStatus), value)
                ? (EScrutinyStatus)value
                : null;
        }

        public static List<EnumModel<short>> GetList()
        {
            return [.. Enum.GetValues(typeof(EScrutinyStatus))
                .Cast<EScrutinyStatus>()
                .Select(e => new EnumModel<short>
                {
                    Value = (short)e,
                    Name = e.GetName()
                })];
        }
    }
}
