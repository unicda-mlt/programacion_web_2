
namespace Domain.API
{
    public class BaseObjectResponse<T> : OkResponse
    {
        public T? Data { get; set; } = default;
    }
}
