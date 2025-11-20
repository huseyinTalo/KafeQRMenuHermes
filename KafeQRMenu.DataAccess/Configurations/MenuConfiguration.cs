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
    public class MenuConfiguration : AuditableEntityConfiguration<Menu>
    {
        public override void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.HasOne(m => m.Cafe)
           .WithMany(c => c.Menus)
           .HasForeignKey(m => m.CafeId)
           .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(m => m.CategoriesOfMenu)  // Menu'daki property adı
               .WithMany(mc => mc.Menus)           // MenuCategory'deki property adı
               .UsingEntity(j => j.ToTable("MenuMenuCategories"));
            builder.Property(x => x.IsActive).HasDefaultValue(false);
            builder.Property(x => x.DisplayDate).HasDefaultValue(DateTime.Now);
            base.Configure(builder);

        }
    }
}
