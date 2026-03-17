using Domain.API.Interfaces;

namespace Domain.API
{
    public class PaginationQueryParams : IPaginationQueryParams
    {
        public int? Page { get; set; } = null;

        public byte? PageSize { get; set; } = null;
        public short? StatusId { get; set; } = null;
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
}
