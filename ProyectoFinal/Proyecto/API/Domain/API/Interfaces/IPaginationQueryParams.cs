
namespace Domain.API.Interfaces
{
    public interface IPaginationQueryParams
    {
        int? Page { get; set; }

        byte? PageSize { get; set; }
    }
}
