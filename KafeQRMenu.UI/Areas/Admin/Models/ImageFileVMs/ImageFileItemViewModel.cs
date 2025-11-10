using KafeQRMenu.Data.Enums;

namespace KafeQRMenu.UI.Areas.Admin.Models.ImageFileVMs
{
    public class ImageFileItemViewModel
    {
        public Guid ImageId { get; set; }
        public string ImageBase64 { get; set; }
        public bool IsActive { get; set; }
        public ImageContentType ImageContentType { get; set; }
        public string ContentTypeName => ImageContentType.ToString();
        public string? RelatedEntityName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public long FileSizeInBytes { get; set; }
        public string FileSize => FormatFileSize(FileSizeInBytes);

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
