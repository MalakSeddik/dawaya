using Dawaya.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dawaya.Api.Data.Configurations;

public class ActiveIngredientConfiguration : IEntityTypeConfiguration<ActiveIngredient>
{
    public void Configure(EntityTypeBuilder<ActiveIngredient> b)
    {
        b.ToTable("ActiveIngredients");
        b.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.NameAr).HasMaxLength(200);
        b.HasIndex(x => x.NameEn).IsUnique();
    }
}

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> b)
    {
        b.ToTable("Medicines");
        b.Property(x => x.BrandNameEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.BrandNameAr).HasMaxLength(200);
        b.Property(x => x.Manufacturer).HasMaxLength(200);
        b.Property(x => x.RegistrationStatus).HasConversion<string>().HasMaxLength(20);

        // Autocomplete in Arabic and English (BR-80)
        b.HasIndex(x => x.BrandNameEn);
        b.HasIndex(x => x.BrandNameAr);
    }
}

public class MedicineProductConfiguration : IEntityTypeConfiguration<MedicineProduct>
{
    public void Configure(EntityTypeBuilder<MedicineProduct> b)
    {
        b.ToTable("MedicineProducts");
        b.Property(x => x.Form).HasMaxLength(50).IsRequired();
        b.Property(x => x.StrengthText).HasMaxLength(100).IsRequired();
        b.Property(x => x.StrengthValue).HasPrecision(18, 4);
        b.Property(x => x.StrengthUnit).HasMaxLength(20);
        b.Property(x => x.Barcode).HasMaxLength(50);
        b.Property(x => x.UnitOfCount).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Medicine).WithMany(m => m.Products)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);

        // EF adds "[Barcode] IS NOT NULL" automatically for SQL Server
        b.HasIndex(x => x.Barcode).IsUnique();
        b.HasIndex(x => new { x.MedicineId, x.Form, x.StrengthText }).IsUnique();
    }
}

public class ProductIngredientConfiguration : IEntityTypeConfiguration<ProductIngredient>
{
    public void Configure(EntityTypeBuilder<ProductIngredient> b)
    {
        b.ToTable("ProductIngredients");
        b.Property(x => x.Amount).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20);

        b.HasOne(x => x.Product).WithMany(p => p.Ingredients)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActiveIngredient).WithMany(i => i.Products)
            .HasForeignKey(x => x.ActiveIngredientId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ProductId, x.ActiveIngredientId }).IsUnique();
    }
}

public class ProductDoseRuleConfiguration : IEntityTypeConfiguration<ProductDoseRule>
{
    public void Configure(EntityTypeBuilder<ProductDoseRule> b)
    {
        b.ToTable("ProductDoseRules");
        b.Property(x => x.AgeGroup).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AgeRangeText).HasMaxLength(100);
        b.Property(x => x.UsualDoseTextAr).HasMaxLength(500);
        b.Property(x => x.UsualDoseTextEn).HasMaxLength(500);
        b.Property(x => x.MaxDailyAmount).HasPrecision(18, 4);
        b.Property(x => x.MaxDailyUnit).HasMaxLength(20);

        b.HasOne(x => x.Product).WithMany(p => p.DoseRules)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.ProductId);
    }
}

public class MedicineContentConfiguration : IEntityTypeConfiguration<MedicineContent>
{
    public void Configure(EntityTypeBuilder<MedicineContent> b)
    {
        b.ToTable("MedicineContents");
        b.Property(x => x.Language).HasConversion<string>().HasMaxLength(5);
        b.Property(x => x.Section).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Body).IsRequired();
        b.Property(x => x.SourceRef).HasMaxLength(500);

        b.HasOne(x => x.Medicine).WithMany(m => m.Contents)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.VerifiedBy).WithMany()
            .HasForeignKey(x => x.VerifiedById).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => new { x.MedicineId, x.Language, x.Section, x.Version }).IsUnique();
    }
}

public class MedicineWarningConfiguration : IEntityTypeConfiguration<MedicineWarning>
{
    public void Configure(EntityTypeBuilder<MedicineWarning> b)
    {
        b.ToTable("MedicineWarnings");
        b.Property(x => x.TargetGroup).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.TextAr).IsRequired();

        b.HasOne(x => x.Medicine).WithMany(m => m.Warnings)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.MedicineId, x.TargetGroup });
    }
}

public class MedicineRecallConfiguration : IEntityTypeConfiguration<MedicineRecall>
{
    public void Configure(EntityTypeBuilder<MedicineRecall> b)
    {
        b.ToTable("MedicineRecalls");
        b.Property(x => x.Batches).HasMaxLength(500);
        b.Property(x => x.SourceRef).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);

        b.HasOne(x => x.Medicine).WithMany(m => m.Recalls)
            .HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        // NoAction avoids multiple cascade paths (Medicine -> Product -> Recall)
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class MedicalTermConfiguration : IEntityTypeConfiguration<MedicalTerm>
{
    public void Configure(EntityTypeBuilder<MedicalTerm> b)
    {
        b.ToTable("MedicalTerms");
        b.Property(x => x.TermAr).HasMaxLength(200).IsRequired();
        b.Property(x => x.TermEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.PlainExplanationAr).HasMaxLength(1000).IsRequired();
        b.HasIndex(x => x.TermEn);
        b.HasIndex(x => x.TermAr);
    }
}

public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> b)
    {
        b.ToTable("Interactions", t =>
            t.HasCheckConstraint("CK_Interactions_IngredientOrder", "[IngredientAId] < [IngredientBId]"));

        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.DescriptionAr).IsRequired();
        b.Property(x => x.Source).HasMaxLength(300);

        // Two FKs to the same table: NoAction to avoid multiple cascade paths
        b.HasOne(x => x.IngredientA).WithMany()
            .HasForeignKey(x => x.IngredientAId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.IngredientB).WithMany()
            .HasForeignKey(x => x.IngredientBId).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => new { x.IngredientAId, x.IngredientBId }).IsUnique();
        b.HasIndex(x => x.IngredientBId);
    }
}
