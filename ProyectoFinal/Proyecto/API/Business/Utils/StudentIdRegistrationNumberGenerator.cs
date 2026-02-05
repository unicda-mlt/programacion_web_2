using Data.Repositories;

namespace Business.Utils
{
    public class StudentIdRegistrationNumberGenerator
    {
        private static readonly short padDigits = 4;
        private static readonly Int32 digitLimit = (Int32)Math.Pow(10, padDigits) - 1;
        private static readonly string dateFormat = "yyyyddMM";

        public async static Task<string> GetNewRegistrationNumber(StudentRepository estudianteRepository)
        {
            string todayDate = DateTime.Now.ToString(dateFormat);

            var lastTodayEstudiante = await estudianteRepository
                .GetOneByFilter(x => x.RegistrationNumber != null && x.RegistrationNumber.StartsWith(todayDate), orderByDescending: x => x.RegistrationNumber);

            if (lastTodayEstudiante == null || lastTodayEstudiante.RegistrationNumber == null)
            {
                string currentDate = DateTime.Now.ToString(dateFormat);
                return currentDate + "1".PadLeft(padDigits, '0');
            }

            string previousIdNumber = lastTodayEstudiante.RegistrationNumber;
            string previousDate = previousIdNumber[..8];
            int sequence = int.Parse(previousIdNumber[8..]) + 1;

            if (sequence > digitLimit)
            {
                string nextDate = DateTime.ParseExact(previousDate, dateFormat, null).AddDays(1).ToString(dateFormat);

                return nextDate + "1".PadLeft(padDigits, '0');
            }

            return $"{previousDate}" + sequence.ToString().PadLeft(padDigits, '0');
        }
    }
}
