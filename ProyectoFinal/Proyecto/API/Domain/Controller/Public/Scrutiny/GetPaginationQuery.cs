using Domain.API;

namespace Domain.Controller.Public.Scrutiny
{
    public class GetPaginationQuery : PaginationQueryParams
    {
        public DateTime? FromStartDate { get; set; } = null;
        public DateTime? ToStartDate { get; set; } = null;
        public DateTime? FromEndDate { get; set; } = null;
        public DateTime? ToEndDate { get; set; } = null;
    }
}
