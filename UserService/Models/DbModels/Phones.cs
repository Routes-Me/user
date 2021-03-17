namespace UserService.Models.DBModels
{
    public partial class Phones
    {
        public int PhoneId { get; set; }
        public string Number { get; set; }
        public bool? IsVerified { get; set; }
        public int? UserId { get; set; }

        public virtual Users User { get; set; }
    }
}
