
namespace Domain.API.Interfaces
{
    public interface IPaginationResponse<T>
    {
        public IPagination Pagination { get; set; }
        public List<T> Data { get; set; }
    }
}
