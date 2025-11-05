namespace KafeQRMenu.UI.Areas.SuperAdmin.Models.AdminViewModels
{
    public class SAAdminUpdateVM
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IdentityId { get; set; }
        public string Password { get; set; }
        public Guid CafeId { get; set; }
    }
}
