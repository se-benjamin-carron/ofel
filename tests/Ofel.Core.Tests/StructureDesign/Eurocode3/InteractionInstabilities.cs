using Microsoft.EntityFrameworkCore.Storage.Json;
using Ofel.Core.Interfaces;
using Ofel.Core.SectionParameter;
using Ofel.Core.StructureDesign.Eurocode3;
using Ofel.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Tests.StructureDesign.Eurocode3
{
    public class InteractionInstabilitiesTests
    {
        private static SteelInteractionInstabilitiesInput CreateInteractionInstabilities(
            SteelSection section,
            SteelMaterial material,
            double Ned,
            double MyEd,
            double MzEd,
            double length,
            double ky,
            double kz,
            double kt,
            double klt)
        {
            var effort = new ForceValue(Ned, 0, 0, 0, MyEd, MzEd);
            FlexionCoefficient c1c2 = FlexionCoefficient.CreateFromDefault(1.77, 0.0);
            var cmi0 = ShapeCoefficient.CreateDefault();
            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());
            var latBuckInput = new SteelLateralBucklingInput()
            {
                Context = context,
                Type = LateralBucklingType.Default,
                Interval = new IntervalLateralBuckling(length, klt),
                IsStabilisingForce = StabilisingForceType.Neutral,
                CoefficientC1C2 = c1c2,
            };
            SteelLateralBuckling latBuck = SteelLateralBucklingFactory.Create(latBuckInput);
            var normalInput = new SteelNormalInput()
            {
                Context = context,
                ReductionCoef = 1.0,
            };
            var normal = new SteelNormal_Stable(normalInput);
            var flexionYInput = new SteelFlexionInput()
            {
                Axis = Axis.Y,
                ReductionCoef = 1.0,
                Context = context,
            };
            var flexionY = SteelFlexionFactory.Create(flexionYInput);
            var flexionZInput = new SteelFlexionInput()
            {
                Axis = Axis.Z,
                ReductionCoef = 1.0,
                Context = context,
            };
            var flexionZ = SteelFlexionFactory.Create(flexionZInput);
            var buckYInput = new SteelBucklingDefaultInput()
            {
                Axis = Axis.Y,
                Interval = new IntervalBuckling { Length = length, Start = 0, End = 1.0, K = ky },
                Context = context,
            };
            var buckY = SteelBucklingFactory.Create(buckYInput);
            var buckZInput = new SteelBucklingDefaultInput()
            {
                Axis = Axis.Z,
                Interval = new IntervalBuckling { Length = length, Start = 0, End = 1.0, K = kz },
                Context = context,
            };
            var buckZ = SteelBucklingFactory.Create(buckZInput);
            var buckTorsionInput = new SteelBucklingTorsionInput()
            {
                BucklingY = buckY,
                BucklingZ = buckZ,
                Interval = new IntervalBuckling { Length = length, Start = 0, End = 1.0, K = kt },
                Context = context,
            };
            var buckTorsion = (SteelBucklingTorsionBase)SteelBucklingFactory.Create(buckTorsionInput);

            var interval = new IntervalLateralBuckling(length, klt, 1.0, 1.0);
            var input = new SteelInteractionInstabilitiesInput()
            {
                Type = InteractionInstabilitiesType.AnnexA,
                Normal = normal,
                FlexionY = flexionY,
                FlexionZ = flexionZ,
                BucklingY = buckY,
                BucklingZ = buckZ,
                Torsion = buckTorsion,
                LateralBuckling = latBuck,
                CoefficientC1C2 = c1c2,
                Cmi0 = cmi0,
                Context = context,
            };
            return input;
        }

        [Theory]
        [InlineData("IPE", "IPE500", "S275", 4564, 101617.8, 12354, 1, 1, 1, 1, 10, 0.998147245, 0.070140711, 0.1862941911, 0.0032086601, 0.02039205265, 0.9879628644, 0.8988041, 0.98953863, 0.9938834, 1.0169981, 0.7742965, 0.523635, 1.0118045, 0.47445130)]
        [InlineData("HEA", "HEA400", "S355", -4984272, -74872, -123214, 1, 2, 1, 0.7, 20, 0.99580646, 0.06134389, 0.6170980, 1.643114, 3.5124461, 1.18597390, 1.5746048, 1.0134376, 0.65415635, 0.843188, 0.443143, 0.5090842, 1.5286865, 1.5617340)]

        public async Task SteelInteractionInsatbilities(string sectionType, string sectionName, string materialName, double Ned, double MyEd, double MzEd,
            double ky, double kz, double kt, double klt, double length, double alt, double blt, double clt, double dlt, double elt,
            double cyy, double cyz, double czy, double czz,
            double kyy, double kyz, double kzy, double kzz, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateInteractionInstabilities(sec!, mat!, Ned, MyEd, MzEd, length, ky, kz, kt, klt);
            var interactionInstabilities = new SteelInteractionInstabilities_AnnexA_12(input);

            // Assert
            Assert.Equal(alt, interactionInstabilities.A_LT, 1e-5);
            Assert.Equal(blt, interactionInstabilities.B_LT, 1e-5);
            Assert.Equal(clt, interactionInstabilities.C_LT, 1e-5);
            Assert.Equal(dlt, interactionInstabilities.D_LT, 1e-5);
            Assert.Equal(elt, interactionInstabilities.E_LT, 1e-5);
            Assert.Equal(cyy, interactionInstabilities.Cyy, 1e-5);
            Assert.Equal(cyz, interactionInstabilities.Cyz, 1e-5);
            Assert.Equal(czy, interactionInstabilities.Czy, 1e-5);
            Assert.Equal(czz, interactionInstabilities.Czz, 1e-5);
            Assert.Equal(kyy, interactionInstabilities.Kyy, 1e-5);
            Assert.Equal(kyz, interactionInstabilities.Kyz, 1e-5);
            Assert.Equal(kzy, interactionInstabilities.Kzy, 1e-5);
            Assert.Equal(kzz, interactionInstabilities.Kzz, 1e-5);
            Assert.Equal(ratio, interactionInstabilities.Ratio, 1e-4);
        }
        [Theory]
        [InlineData("IPE", "IPE500", "S275", 4564, 101617.8, 12354, 1, 1, 1, 1, 10, 1.004757, 1.010352, 0.995723, 1.001268, 0.50716)]
        [InlineData("HEA", "HEA400", "S355", -4984272, -74872, -123214, 1, 2, 1, 0.7, 20, 1.0, 1.0, 1.0, 1.0, 1.41972044)]
        public async Task SteelInteractionInstabilitiesClass3(string sectionType, string sectionName, string materialName, double Ned, double MyEd, double MzEd,
             double ky, double kz, double kt, double klt, double length,
             double kyy, double kyz, double kzy, double kzz, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateInteractionInstabilities(sec!, mat!, Ned, MyEd, MzEd, length, ky, kz, kt, klt);
            var interactionInstabilities = new SteelInteractionInstabilities_AnnexA_3(input);

            // Assert
            Assert.Equal(kyy, interactionInstabilities.Kyy, 1e-5);
            Assert.Equal(kyz, interactionInstabilities.Kyz, 1e-5);
            Assert.Equal(kzy, interactionInstabilities.Kzy, 1e-5);
            Assert.Equal(kzz, interactionInstabilities.Kzz, 1e-5);
            Assert.Equal(ratio, interactionInstabilities.Ratio, 1e-4);
        }

    }
}
