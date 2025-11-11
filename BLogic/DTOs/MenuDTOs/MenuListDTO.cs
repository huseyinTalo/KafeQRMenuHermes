using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuDTOs
{
    public class MenuListDTO
    {
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public bool IsActive { get; set; }
        public Guid? ImageFileId { get; set; }
        public Guid CafeId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string CafeName { get; set; } // ← ADD THIS
        public DateTime CreatedTime { get; set; } // ← ADD THIS
        public DateTime? UpdatedTime { get; set; }
    }
}
