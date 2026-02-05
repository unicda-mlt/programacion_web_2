
namespace Business.Utils
{
    public static class EnumHandler
    {
        public static IEnumerable<object> ToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new
            {
                Nombre = e.ToString(),
                Valor = Convert.ToInt32(e)
            });
        }
    }
}
