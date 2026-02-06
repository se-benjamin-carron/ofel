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
    public class FlexionTests
    {
        private static SteelFlexionInput CreateFlexionYInput(
            SteelSection section,
            SteelMaterial material,
            double Med,
            double coefReduction)
        {
            var effort = new ForceValue(0, 0, 0, 0, Med, 0);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());

            return new SteelFlexionInput
            {
                Context = context,
                ReductionCoef = coefReduction,
                Axis = Axis.Y

            };
        }

        private static SteelFlexionInput CreateFlexionZInput(
            SteelSection section,
            SteelMaterial material,
            double Med,
            double coefReduction)
        {
            var effort = new ForceValue(0, 0, 0, 0, 0, Med);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());

            return new SteelFlexionInput
            {
                Context = context,
                ReductionCoef = coefReduction,
                Axis = Axis.Z

            };
        }

        [Theory]
        [InlineData("IPE300", "S275", 135465, 1, 0.783896855341)]
        [InlineData("IPE500", "S355", -825452, 1, 1.059680716)]
        public async Task SteelFlexion12_Y_Resistance(string sectionName, string materialName, double Med, double coef, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", sectionName);

            var input = CreateFlexionYInput(sec!, mat!, Med, coef);
            var flexion = new SteelFlexionResult_Class12_Y(input);

            // Assert
            Assert.Equal(ratio, flexion.Ratio, 1e-4);
        }

        [Theory]
        [InlineData("IPE200", "S355", 40000, 0.5, 1.15971381914214)]
        [InlineData("IPE500", "S275", -100000, 0.2, 0.943069710)]
        public async Task SteelFlexion3_Y_Resistance(string sectionName, string materialName, double Med, double coef, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", sectionName);

            var input = CreateFlexionYInput(sec!, mat!, Med, coef);
            var flexion = new SteelFlexionResult_Class3_Y(input);

            // Assert
            Assert.Equal(ratio, flexion.Ratio, 1e-4);
        }

        [Theory]
        [InlineData("IPE200", "S355", 64290, 0.5, 8.121013073)]
        [InlineData("IPE500", "S275", -1000000, 1, 10.8261517)]
        public async Task SteelFlexion12_Z_Resistance(string sectionName, string materialName, double Med, double coef, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", sectionName);

            var input = CreateFlexionZInput(sec!, mat!, Med, coef);
            var flexion = new SteelFlexionResult_Class12_Z(input);

            // Assert
            Assert.Equal(ratio, flexion.Ratio, 1e-4);
        }

        [Theory]
        [InlineData("IPE200", "S355", 135465, 1, 13.38917716)]
        [InlineData("IPE500", "S275", -1000000, 1, 16.992353)]
        public async Task SteelFlexion3_Z_Resistance(string sectionName, string materialName, double Med, double coef, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", sectionName);

            var input = CreateFlexionZInput(sec!, mat!, Med, coef);
            var flexion = new SteelFlexionResult_Class3_Z(input);

            // Assert
            Assert.Equal(ratio, flexion.Ratio, 1e-4);
        }


    }

}
