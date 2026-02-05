
namespace Business.Utils
{
    public class MultimediaMetaGenerator
    {
        public static string GenerateEpochFilename()
        {
            long epochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Random random = new();
            int randomNumber = random.Next(10000, 100000);
            return $"{epochSeconds}_{randomNumber}";
        }
    }
}
