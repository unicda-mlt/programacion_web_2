using System.Text;

namespace API
{
    public class SwaggerHelper
    {
        public static string SafeSchemaId(Type t)
        {
            string raw = BuildId(t);
            var sb = new StringBuilder(raw.Length);
            foreach (var ch in raw)
                sb.Append(char.IsLetterOrDigit(ch) || ch == '.' || ch == '_' || ch == '-' ? ch : '_');
            return sb.ToString();
        }

        public static string BuildId(Type t)
        {
            if (t.IsGenericType)
            {
                string ns = t.Namespace ?? "";
                string name = t.GetGenericTypeDefinition().Name.Split('`')[0];
                string args = string.Join("_", t.GetGenericArguments().Select(BuildId));
                return $"{ns}.{name}_{args}".Replace("+", ".");
            }
            return (t.FullName ?? t.Name).Replace("+", ".");
        }
    }
}
