using KafeQRMenu.Data.Core.BaseEntityConfigurations;
using KafeQRMenu.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Configurations
{
    public class MenuItemConfiguration : AuditableEntityConfiguration<MenuItem>
    {
        public override void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.Property(x => x.MenuItemName).IsRequired().HasMaxLength(128);
            builder.Property(x => x.Price).IsRequired();
            base.Configure(builder);
        }
    }
}
