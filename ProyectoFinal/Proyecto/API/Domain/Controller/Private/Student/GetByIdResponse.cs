
using Domain.API;

namespace Domain.Controller.Private.Student
{
    public class GetByIdResponse
    {   
        public class Response : BaseObjectResponse<Data> { }

        public class Data
        {
            public required Guid Id { get; set; }
            public Guid? UserId { get; set; }
            public required string RegistrationNumber { get; set; }
            public required string Name { get; set; }
            public required string LastName { get; set; }
            public required bool Graduated { get; set; }
            public required DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}
