using Xunit;
using Ofel.Infrastructure.Repositories;
using Ofel.Infrastructure.Utils;

namespace Ofel.Infrastructure.Tests
{
    public class DbFactoryTests
    {
        [Fact]
        public async Task Can_Create_Db_From_Csv()
        {
            // Arrange & Act
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            // Assert : la DB doit contenir des matériaux et sections
            var hasAny = await repo.HasAnyAsync();
            Assert.True(hasAny);

            var mat = await repo.GetMaterialAsync("S235", "NF EN 10025-2");
            Assert.NotNull(mat);
            Assert.Equal("S235", mat!.Name);

            var sec = await repo.GetSectionAsync("IPE", "IPE200");
            Assert.NotNull(sec);
            Assert.Equal("IPE200", sec!.Name);
        }
    }
}
