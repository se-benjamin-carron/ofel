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
    public class InteractionTests
    {
        private static SteelInteractionInput CreateInteractionInput(
            SteelSection section,
            SteelMaterial material,
            double Ned,
            double MyEd,
            double MzEd,
            double coefReduction)
        {
            var effort = new ForceValue(Ned, 0, 0, 0, MyEd, MzEd);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());
            SteelNormal_Stable normal = new SteelNormal_Stable(new SteelNormalInput { Context = context, ReductionCoef = coefReduction });
            SteelFlexionResult_Class12_Y flexionY = new SteelFlexionResult_Class12_Y(new SteelFlexionInput { Context = context, Axis = Axis.Y, ReductionCoef = coefReduction });
            SteelFlexionResult_Class12_Z flexionZ = new SteelFlexionResult_Class12_Z(new SteelFlexionInput { Context = context, Axis = Axis.Z, ReductionCoef = coefReduction });
            return new SteelInteractionInput
            {
                Normal = normal,
                FlexionY = flexionY,
                FlexionZ = flexionZ,
                Context = context

            };
        }

        [Theory]
        [InlineData("IPE", "IPE200", "S355", -12345, 124567, 12345, 1, 78333.235, 15833, 2, 1, 3.308497162934)]
        [InlineData("TCAR", "TCAR100x4", "S275", -123456, 12345, 3541, 1, 13360.4899, 13360.4899, 1.8483934, 1.8483934, 0.949965)]
        public async Task SteelInteraction12_Resistance(string sectionType, string sectionName, string materialName, double Ned, double MyEd, double MzEd, double CoefReduction, double MnyRd, double MnzRd, double alpha, double beta, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateInteractionInput(sec!, mat!, Ned, MyEd, MzEd, CoefReduction);
            var interaction = (SteelInteraction_12)SteelInteractionFactory.Create(input);

            // Assert
            Assert.Equal(MnyRd, interaction.M_N_Y_Rd, 0.1);
            Assert.Equal(MnzRd, interaction.M_N_Z_Rd, 0.1);
            Assert.Equal(alpha, interaction.alpha, 1e-4);
            Assert.Equal(beta, interaction.beta, 1e-4);
            Assert.Equal(ratio, interaction.Ratio, 1e-4);
        }
        [Theory]
        [InlineData("IPE", "IPE200", "S355", 123457, -574221, -6514, 1, 43342426.12545, -2959902061.855, -229366197.183, 3232610685.16422, 9.105945592)]
        [InlineData("TCAR", "TCAR100x4", "S275", 12345, -574, -651, 1, 8258629.917, -12699115.04, -14402654.8672, 35360399.82855, 0.128583272)]

        public async Task SteelInteraction3_Resistance(string sectionType, string sectionName, string materialName, double Ned, double MyEd, double MzEd, double CoefReduction, double SigmaN, double SigmaMy, double SigmaMz, double Sigma, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateInteractionInput(sec!, mat!, Ned, MyEd, MzEd, CoefReduction);
            var interaction = new SteelInteraction_Class3(input);

            // Assert
            Assert.Equal(SigmaN, interaction.Sigma_N, 1);
            Assert.Equal(SigmaMz, interaction.Sigma_M_zEd, 1);
            Assert.Equal(SigmaMy, interaction.Sigma_M_yEd, 1);
            Assert.Equal(Sigma, interaction.Sigma, 1);
            Assert.Equal(ratio, interaction.Ratio, 1e-4);
        }

    }

}
