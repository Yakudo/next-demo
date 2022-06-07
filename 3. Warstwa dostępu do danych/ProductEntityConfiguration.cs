using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yakudo.Next.Domain.Common.ExpandoModels;
using Yakudo.Next.Domain.Common.GeneratedNumbers;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.Next.Domain.Products.ProductTypes;

// Konfiguracja entity Product w Entity framework. To mówi jakie sa kolumny i relacje w bazie
namespace Yakudo.Next.DataAccess.WriteSide.Products
{
    public class ProductEntityConfiguration : AggregateRootEntityTypeConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);
            builder.HasOne<ExpandoData>().WithMany().HasForeignKey(m => m.AttributesDataId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.Name).IsRequired();
            builder.Property(m => m.Code).HasMaxLength(255).IsRequired();
            builder.HasIndex(m => m.Code).IsUnique();
            builder.Property(m => m.PkwiuCode).HasMaxLength(255);
            builder.Property(m => m.CnCode).HasMaxLength(255);
            builder.Property(m => m.CatalogNumber).HasMaxLength(255);

            builder.Property(m => m.DefaultQuantity).HasPrecision(15, 6);
            builder.Property(m => m.Depth).HasPrecision(15, 6);
            builder.Property(m => m.Width).HasPrecision(15, 6);
            builder.Property(m => m.Height).HasPrecision(15, 6);
            builder.Property(m => m.NetWeight).HasPrecision(15, 6);
            builder.Property(m => m.Tare).HasPrecision(15, 6);

            builder.HasOne(m => m.Category).WithMany().IsRequired(false).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<NumberGeneratorTemplate>().WithMany().HasForeignKey(m => m.NumberGeneratorTemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.PurchaseTaxRate).WithMany().HasForeignKey(m => m.PurchaseTaxRateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.SalesTaxRate).WithMany().HasForeignKey(m => m.SalesTaxRateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.BaseUnit).WithMany().HasForeignKey(m => m.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.SizeUnit).WithMany().HasForeignKey(m => m.SizeUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.WeightUnit).WithMany().HasForeignKey(m => m.WeightUnitId).OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(m => m.AlternativeUnits);
            builder.HasMany(typeof(ProductAlternativeUnit), "_alternativeUnits").WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.HandlingUnitItemDataModel).WithMany().HasForeignKey(m => m.HandlingUnitItemDataModelId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
