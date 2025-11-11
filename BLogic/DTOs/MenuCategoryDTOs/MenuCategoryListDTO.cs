using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs
{
    public class MenuCategoryListDTO
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public string CafeName { get; set; }
        public Guid CafeId { get; set; }
        public Guid? ImageFileId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public byte[] ImageFileBytes { get; set; }
        public int ItemCount { get; set; }
    }
}
