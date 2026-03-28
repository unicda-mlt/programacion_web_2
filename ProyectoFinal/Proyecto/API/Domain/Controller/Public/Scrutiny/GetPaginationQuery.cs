using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class GetPaginationQuery : PaginationQueryParams
    {
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
}
