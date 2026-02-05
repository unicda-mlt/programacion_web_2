using Domain.API.Interfaces;

namespace Domain.API
{
    public class PaginationResponse<T> : OkResponse, IPaginationResponse<T>
    {
        public required IPagination Pagination { get; set; }
        public required List<T> Data { get; set; }
    }
}
