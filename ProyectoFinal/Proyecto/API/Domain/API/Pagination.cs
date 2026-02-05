using Domain.API.Interfaces;

namespace Domain.API
{
    public class Pagination : IPagination
    {
        public int Pages { get; set; } = 0;
        public int Records { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
        public int PrevPage { get; set; } = 0;
        public int NextPage { get; set; } = 0;
    }
}
