namespace KafeQRMenu.UI.Models.CafeVMs
{
    public class CafeVM
    {
        public Guid CafeId { get; set; }
        public string CafeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string DomainName { get; set; }
    }
}
