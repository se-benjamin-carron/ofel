using System;
using Xunit;
using Ofel.Core.Utils;
using Ofel.Core.Interfaces;
using Ofel.Infrastructure.Utils;
using Ofel.Core.SectionParameter;
namespace Ofel.Core.Tests.Utils
{
    public class UtilsTests
    {
        private const double TOL = 1e-12;

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(90.0, Math.PI / 2)]
        [InlineData(180.0, Math.PI)]
        [InlineData(360.0, 2 * Math.PI)]
        [InlineData(-90.0, -Math.PI / 2)]
        public void ToRadians_ConvertsDegreesCorrectly(double degrees, double expectedRadians)
        {
            // Act
            var radians = Calculator.ToRadians(degrees);

            // Assert
            Assert.Equal(expectedRadians, radians, TOL);
        }
    }
    public class DbTests
    {

        public class SteelRepositoryTests
        {
            [Fact]
            public async Task Can_Load_Full_Db_And_Read_SteelData()
            {
                // Arrange : Crée la DB complète en mémoire depuis les CSV
                ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

                // Act : Vérifie qu'il y a des données
                bool hasAny = await repo.HasAnyAsync();

                // Lis un matériau connu
                SteelMaterial? mat = await repo.GetMaterialAsync("S235", "NF EN 10025-2");

                // Lis une section connue
                SteelSection? sec = await repo.GetSectionAsync("IPE", "IPE100");

                // Assert : vérifications simples
                Assert.True(hasAny, "La DB doit contenir des données");
                Assert.NotNull(mat);
                Assert.Equal("S235", mat!.Name);
                Assert.Equal(235000000, mat.Fy);

                Assert.NotNull(sec);
                Assert.Equal("IPE100", sec!.Name);
                Assert.Equal(0.1, sec.H);

                SteelSection? sec2 = await repo.GetSectionAsync("TCAR", "TCAR100x4");
                Assert.NotNull(sec2);
                Assert.Equal("TCAR100x4", sec2!.Name);
                Assert.Equal(0.1, sec2.H);
            }
        }
    }
}
