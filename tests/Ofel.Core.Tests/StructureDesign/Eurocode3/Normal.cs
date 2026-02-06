using Ofel.Core.Interfaces;
using Ofel.Core.SectionParameter;
using Ofel.Core.StructureDesign.Eurocode3;
using Ofel.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Tests.StructureDesign.Eurocode3
{
    public class NormalTests
    {
        private static SteelNormalInput CreateNormalInput(
            SteelSection section,
            SteelMaterial material,
            double ned,
            double coefReduction)
        {
            var effort = new ForceValue(ned, 0, 0);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());

            return new SteelNormalInput
            {
                Context = context,
                ReductionCoef = coefReduction

            };
        }

        [Theory]
        [InlineData("IPE200", "S355", 135465, 1, 0.1339858)]
        [InlineData("IPE500", "S275", -109827, 0.1, 0.0345776)]
        public async Task SteelNormal_Resistance(string sectionName, string materialName, double Ned, double coef, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", sectionName);

            var input = CreateNormalInput(sec!, mat!, Ned, coef);
            var normal = new SteelNormal_Stable(input);

            // Assert
            Assert.Equal(ratio, normal.Ratio, 1e-4);
        }


    }

}
