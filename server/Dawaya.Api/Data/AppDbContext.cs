using Dawaya.Api.Domain;
using Dawaya.Api.Domain.Catalog;
using Dawaya.Api.Domain.Identity;
using Dawaya.Api.Domain.Profiles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dawaya.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Profiles
    public DbSet<PatientProfile> Profiles => Set<PatientProfile>();
    public DbSet<ProfileAllergy> ProfileAllergies => Set<ProfileAllergy>();

    // Catalog
    public DbSet<ActiveIngredient> ActiveIngredients => Set<ActiveIngredient>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineProduct> MedicineProducts => Set<MedicineProduct>();
    public DbSet<ProductIngredient> ProductIngredients => Set<ProductIngredient>();
    public DbSet<ProductDoseRule> ProductDoseRules => Set<ProductDoseRule>();
    public DbSet<MedicineContent> MedicineContents => Set<MedicineContent>();
    public DbSet<MedicineWarning> MedicineWarnings => Set<MedicineWarning>();
    public DbSet<MedicineRecall> MedicineRecalls => Set<MedicineRecall>();
    public DbSet<MedicalTerm> MedicalTerms => Set<MedicalTerm>();
    public DbSet<Interaction> Interactions => Set<Interaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private void StampTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var e in ChangeTracker.Entries<BaseEntity>())
        {
            if (e.State == EntityState.Added) { e.Entity.CreatedAt = now; e.Entity.UpdatedAt = now; }
            else if (e.State == EntityState.Modified) e.Entity.UpdatedAt = now;
        }
    }
}
