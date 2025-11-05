namespace KafeQRMenu.UI.Areas.SuperAdmin.Models.AdminViewModels
{
    public class SAAdminCreateVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Guid CafeId { get; set; }
    }
}
