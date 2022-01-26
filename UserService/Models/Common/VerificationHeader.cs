using System;

namespace UserService.Models.Common
{
    public class VerificationHeader
    {
        public string Subject { get; set; }
        public DateTime ExpiryTime { get; set; }    
    }
}
