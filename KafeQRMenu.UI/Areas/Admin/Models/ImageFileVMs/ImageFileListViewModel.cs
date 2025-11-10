using KafeQRMenu.Data.Enums;

namespace KafeQRMenu.UI.Areas.Admin.Models.ImageFileVMs
{
    public class ImageFileListViewModel
    {
        public List<ImageFileItemViewModel> Images { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public ImageContentType? TypeFilter { get; set; }
        public bool? ActiveFilter { get; set; }
    }
}
