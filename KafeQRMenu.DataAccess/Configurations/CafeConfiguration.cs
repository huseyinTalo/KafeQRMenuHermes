using KafeQRMenu.Data.Core.BaseEntityConfigurations;
using KafeQRMenu.Data.Core.Concrete;
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
    public class CafeConfiguration : AuditableEntityConfiguration<Cafe>
    {
        public override void Configure(EntityTypeBuilder<Cafe> builder)
        {
            builder.Property(a => a.CafeName)
                   .IsRequired()
                   .HasMaxLength(128);

            builder.Property(a => a.Address)
                   .IsRequired()
                   .HasMaxLength(128);


            base.Configure(builder);
        }
    }
}
