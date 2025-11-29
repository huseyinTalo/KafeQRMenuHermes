namespace KafeQRMenu.UI.Areas.Admin.Models.AdminVMs
{
    public class AdminCafeVM
    {
        public Guid Id { get; set; }
        public string CafeName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string DomainName { get; set; }
        public Guid ImageFileId { get; set; }
        public byte[] ImageFileBytes { get; set; }
    }
}
