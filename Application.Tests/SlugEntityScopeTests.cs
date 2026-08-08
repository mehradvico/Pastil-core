using Entities.Entities;
using Entities.Entities.CommonField;
using Entities.Entities.Security;
using Xunit;

namespace Application.Tests;

public class SlugEntityScopeTests
{
    [Fact]
    public void Slug_IsLimitedToContentAndStoreEntities()
    {
        var slugEntities = new ISlugEntity[]
        {
            new Role { Label = "Admin" },
            new Banner { Label = "Home Banner" },
            new Brand { SecondName = "Royal Canin" },
            new Category { Label = "Dog Food" },
            new Feature { Label = "Package Weight" },
            new Gallery { Label = "Pet Gallery" },
            new PetBreed { Label = "Golden Retriever" },
            new Pet { Label = "Dog" },
            new Product { ProductLabel = "Adult Dog Food" }
        };

        var sources = slugEntities.Select(x => x.GetSlugSource()).ToArray();

        Assert.Equal(
            [
                "Admin",
                "Home Banner",
                "Royal Canin",
                "Dog Food",
                "Package Weight",
                "Pet Gallery",
                "Golden Retriever",
                "Dog",
                "Adult Dog Food"
            ],
            sources);
    }

    [Fact]
    public void TechnicalLabelEntities_DoNotParticipateInSlugSystem()
    {
        Assert.False(typeof(ISlugEntity).IsAssignableFrom(typeof(AdminSetting)));
        Assert.False(typeof(ISlugEntity).IsAssignableFrom(typeof(Code)));
        Assert.False(typeof(ISlugEntity).IsAssignableFrom(typeof(CodeGroup)));
        Assert.False(typeof(ISlugEntity).IsAssignableFrom(typeof(Permission)));
    }
}
