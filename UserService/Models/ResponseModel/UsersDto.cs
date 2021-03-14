using System;

namespace UserService.Models.ResponseModel
{
    public class UsersDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
