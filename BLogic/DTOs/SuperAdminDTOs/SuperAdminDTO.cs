using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.SuperAdminDTOs
{
    public class SuperAdminDTO
    {
        public Guid SuperAdminId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Guid? ImageFileId { get; set; }
    }
}
