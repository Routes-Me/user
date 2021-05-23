using System;

namespace UserService.Models.ResponseModel
{
    public class UsersDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
