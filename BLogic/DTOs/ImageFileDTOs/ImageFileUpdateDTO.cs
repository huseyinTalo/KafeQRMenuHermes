using KafeQRMenu.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.ImageFileDTOs
{
    public class ImageFileUpdateDTO
    {
        public Guid ImageId { get; set; }
        public byte[] ImageByteFile { get; set; }
        public bool IsActive { get; set; }
        public ImageContentType ImageContentType { get; set; }
        public Guid? AdminId { get; set; }
        public Guid? SuperAdminId { get; set; }
        public Guid? MenuCategoryId { get; set; }
        public Guid? MenuItemId { get; set; }
        public Guid? CafeId { get; set; }
    }
}
