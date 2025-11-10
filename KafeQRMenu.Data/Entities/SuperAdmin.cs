using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Entities
{
    public class SuperAdmin : AuditableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IdentityId { get; set; }
        public Guid? SuperAdminImageId { get; set; }
    }
}
