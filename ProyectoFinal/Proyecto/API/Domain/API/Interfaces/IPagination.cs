
namespace Domain.API.Interfaces
{
    public interface IPagination
    {
        int Pages { get; set; }
        int Records { get; set; }
        int CurrentPage { get; set; }
        int PrevPage { get; set; }
        int NextPage { get; set; }
    }
}
