
namespace Domain
{
    public class EnumModel<IdType>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public IdType Value { get; set; }
        public required string Name { get; set; }
    }
}
