using Domain.API;

namespace Domain.Controller.Private.Scrutiny
{
    public class GetPaginationQuery : PaginationQueryParams
    {
        public short? StatusId { get; set; } = null;
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
}
