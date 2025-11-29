namespace KafeQRMenu.UI.Areas.Admin.Models.AdminVMs
{
    public class AdminProfileVM
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public byte[] ImageFileBytes { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid ImageId { get; set; }
        public Guid CafeId { get; set; }
        public string CafeName { get; set; }
    }
}
