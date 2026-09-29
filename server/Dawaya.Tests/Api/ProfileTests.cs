using System.Net;
using Dawaya.Api.Domain;
using Dawaya.Api.Domain.Catalog;
using Dawaya.Api.Features.Profiles;
using Dawaya.Tests.Infrastructure;

namespace Dawaya.Tests.Api;

// BR-06 (family profiles), BR-07 (details that affect warnings only).
public sealed class ProfileTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static SaveProfileRequest Mom(AgeGroup? age = AgeGroup.Elderly) => new("Mom", "Mother", age, false, false);

    [Fact]
    public async Task Create_get_update_and_delete_a_family_profile()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;

        using var created = await client.PostJsonAsync("/api/v1/profiles", Mom());
        await created.EnsureStatusAsync(HttpStatusCode.OK);
        var mom = await created.ReadAsync<ProfileResponse>();
        Assert.False(mom.IsSelf);
        Assert.Equal(AgeGroup.Elderly, mom.AgeGroup);

        using var updated = await client.PutJsonAsync($"/api/v1/profiles/{mom.Id}",
            new SaveProfileRequest("  Mama  ", null, AgeGroup.Adult, IsPregnant: false, IsBreastfeeding: true));
        var changed = await updated.ReadAsync<ProfileResponse>();
        Assert.Equal("Mama", changed.DisplayName);
        Assert.Null(changed.Relationship);
        Assert.True(changed.IsBreastfeeding);

        using var fetched = await client.GetAsync($"/api/v1/profiles/{mom.Id}");
        Assert.Equal("Mama", (await fetched.ReadAsync<ProfileResponse>()).DisplayName);

        using var deleted = await client.DeleteAsync($"/api/v1/profiles/{mom.Id}");
        await deleted.EnsureStatusAsync(HttpStatusCode.NoContent);
        using var gone = await client.GetAsync($"/api/v1/profiles/{mom.Id}");
        await gone.AssertProblemAsync(HttpStatusCode.NotFound, "profile_not_found");
    }

    [Fact]
    public async Task List_puts_me_first_then_family_in_creation_order()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        (await client.PostJsonAsync("/api/v1/profiles", Mom())).Dispose();
        (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Dad", "Father", null, false, false))).Dispose();

        var list = await (await client.GetAsync("/api/v1/profiles")).ReadAsync<List<ProfileResponse>>();

        Assert.Equal(["Me", "Mom", "Dad"], list.Select(p => p.DisplayName));
    }

    [Fact]
    public async Task The_me_profile_cannot_be_deleted()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var me = (await (await client.GetAsync("/api/v1/profiles")).ReadAsync<List<ProfileResponse>>()).Single();

        using var response = await client.DeleteAsync($"/api/v1/profiles/{me.Id}");

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "self_profile_cannot_be_deleted");
    }

    [Fact]
    public async Task An_account_can_have_at_most_20_profiles()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        for (var i = 1; i < ProfileService.MaxProfilesPerAccount; i++)
        {
            using var ok = await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest($"P{i}", null, null, false, false));
            await ok.EnsureStatusAsync(HttpStatusCode.OK);
        }

        using var tooMany = await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("One too many", null, null, false, false));

        await tooMany.AssertProblemAsync(HttpStatusCode.Conflict, "profile_limit_reached");
    }

    [Fact]
    public async Task Invalid_profile_input_is_rejected()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;

        using var empty = await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("  ", null, null, false, false));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);

        using var longName = await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest(new string('x', 101), null, null, false, false));
        Assert.Equal(HttpStatusCode.BadRequest, longName.StatusCode);
    }

    [Fact]
    public async Task Pregnancy_flags_are_allowed_on_any_age_group()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;

        using var response = await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Teen", null, AgeGroup.Child, IsPregnant: true, IsBreastfeeding: false));

        Assert.True((await response.ReadAsync<ProfileResponse>()).IsPregnant);
    }

    [Fact]
    public async Task Allergies_link_to_ingredients_by_choice_or_exact_name_and_otherwise_stay_as_text()
    {
        var penicillinId = await SeedIngredientAsync("Penicillin", "بنسلين");
        var aspirinId = await SeedIngredientAsync("Aspirin", "أسبرين");
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var mom = await (await client.PostJsonAsync("/api/v1/profiles", Mom())).ReadAsync<ProfileResponse>();
        var url = $"/api/v1/profiles/{mom.Id}/allergies";

        // Chosen from suggestions
        (await client.PostJsonAsync(url, new AddAllergyRequest(aspirinId, null))).Dispose();
        // Typed text that exactly matches (any case) is linked, so the red warning can fire (BR-25)
        (await client.PostJsonAsync(url, new AddAllergyRequest(null, "  PENICILLIN "))).Dispose();
        // No match: stays as text
        using var last = await client.PostJsonAsync(url, new AddAllergyRequest(null, "Peanuts"));
        var allergies = (await last.ReadAsync<ProfileResponse>()).Allergies;

        Assert.Collection(allergies,
            a => { Assert.Equal(aspirinId, a.IngredientId); Assert.Equal("أسبرين", a.IngredientNameAr); },
            a => { Assert.Equal(penicillinId, a.IngredientId); Assert.Equal("Penicillin", a.IngredientNameEn); },
            a => { Assert.Null(a.IngredientId); Assert.Equal("Peanuts", a.AllergyText); });

        // Arabic exact match links too
        var dad = await (await client.PostJsonAsync("/api/v1/profiles", new SaveProfileRequest("Dad", null, null, false, false))).ReadAsync<ProfileResponse>();
        using var arabic = await client.PostJsonAsync($"/api/v1/profiles/{dad.Id}/allergies", new AddAllergyRequest(null, "بنسلين"));
        Assert.Equal(penicillinId, Assert.Single((await arabic.ReadAsync<ProfileResponse>()).Allergies).IngredientId);
    }

    [Fact]
    public async Task Duplicate_missing_and_empty_allergies_are_rejected()
    {
        var ibuprofenId = await SeedIngredientAsync("Ibuprofen", null);
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var mom = await (await client.PostJsonAsync("/api/v1/profiles", Mom())).ReadAsync<ProfileResponse>();
        var url = $"/api/v1/profiles/{mom.Id}/allergies";

        (await client.PostJsonAsync(url, new AddAllergyRequest(ibuprofenId, null))).Dispose();
        using var duplicate = await client.PostJsonAsync(url, new AddAllergyRequest(null, "ibuprofen"));
        await duplicate.AssertProblemAsync(HttpStatusCode.Conflict, "allergy_already_added");

        using var missing = await client.PostJsonAsync(url, new AddAllergyRequest(999_999, null));
        await missing.AssertProblemAsync(HttpStatusCode.BadRequest, "ingredient_not_found");

        using var empty = await client.PostJsonAsync(url, new AddAllergyRequest(null, "   "));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
    }

    [Fact]
    public async Task An_allergy_can_be_removed()
    {
        var (client, _) = await factory.CreateSignedInClientAsync();
        using var _c = client;
        var mom = await (await client.PostJsonAsync("/api/v1/profiles", Mom())).ReadAsync<ProfileResponse>();
        var withAllergy = await (await client.PostJsonAsync($"/api/v1/profiles/{mom.Id}/allergies", new AddAllergyRequest(null, "Latex"))).ReadAsync<ProfileResponse>();
        var allergyId = Assert.Single(withAllergy.Allergies).Id;

        using var removed = await client.DeleteAsync($"/api/v1/profiles/{mom.Id}/allergies/{allergyId}");
        await removed.EnsureStatusAsync(HttpStatusCode.NoContent);

        Assert.Empty((await (await client.GetAsync($"/api/v1/profiles/{mom.Id}")).ReadAsync<ProfileResponse>()).Allergies);
    }

    private Task<int> SeedIngredientAsync(string nameEn, string? nameAr) => factory.WithDbAsync(async db =>
    {
        var existing = db.ActiveIngredients.FirstOrDefault(i => i.NameEn == nameEn);
        if (existing is not null) return existing.Id;
        var ingredient = new ActiveIngredient { NameEn = nameEn, NameAr = nameAr };
        db.ActiveIngredients.Add(ingredient);
        await db.SaveChangesAsync();
        return ingredient.Id;
    });
}
