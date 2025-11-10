using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.AdminDTOs
{
    public class AdminListDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IdentityId { get; set; }
        public string CafeName { get; set; }
        public Guid? ImageFileId { get; set; }
        public Guid CafeId { get; set; }
    }
}
