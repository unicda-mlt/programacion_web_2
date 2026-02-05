
namespace Domain.API
{
    public class EnumResponse<T> where T : Enum
    {
        public required bool Ok { get; set; } = true;
        public required DataResponse[] Data { get; set; } = [];

        public class DataResponse
        {
            public required string Nombre { get; set; }
            public required T Valor { get; set; }
        }
    }
}
