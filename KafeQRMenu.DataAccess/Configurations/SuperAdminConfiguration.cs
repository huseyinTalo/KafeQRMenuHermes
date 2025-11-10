using KafeQRMenu.Data.Core.BaseEntityConfigurations;
using KafeQRMenu.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Configurations
{
    public class SuperAdminConfiguration : AuditableEntityConfiguration<SuperAdmin>
    {
        public override void Configure(EntityTypeBuilder<SuperAdmin> builder)
        {
            builder.Property(a => a.FirstName).IsRequired().HasMaxLength(128);
            builder.Property(a => a.LastName).IsRequired().HasMaxLength(128);
            builder.Property(a => a.Email).IsRequired().HasMaxLength(128);
            builder.Property(a => a.IdentityId).IsRequired();
            base.Configure(builder);

        }
    }
}
