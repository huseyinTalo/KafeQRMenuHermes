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
    public class MenuCategoryConfiguration: AuditableEntityConfiguration<MenuCategory>
    {
        public override void Configure(EntityTypeBuilder<MenuCategory> builder)
        {
            builder.Property(x => x.MenuCategoryName).IsRequired().HasMaxLength(128);
            builder.Property(x => x.SortOrder).IsRequired();
            base.Configure(builder);
        }
    }
}
