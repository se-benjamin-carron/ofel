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
    public class LateralBucklingTests
    {
        private static SteelLateralBucklingInput CreateLateralBucklingInput(
            SteelSection section,
            SteelMaterial material,
            double Med,
            double length,
            double coefBuckling,
            double k_w,
            double k_z,
            double c1,
            double c2,
            LateralBucklingType type,
            StabilisingForceType stabilisingForce)
        {
            var effort = new ForceValue(0, 0, 0, 0, Med, 0);
            FlexionCoefficient C1C2 = FlexionCoefficient.CreateFromDefault(c1, c2);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());
            var interval = new IntervalLateralBuckling(length, coefBuckling, k_w, k_z);
            var input = new SteelLateralBucklingInput
            {
                Type = type,
                CoefficientC1C2 = FlexionCoefficient.CreateFromDefault(c1, c2),
                Interval = new IntervalLateralBuckling(length, coefBuckling, k_w, k_z),
                Context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient()),
                IsStabilisingForce = stabilisingForce
            };
            return input;
        }

        [Theory]
        [InlineData("IPE", "IPE200", "S355", 123456, 1, 1, 1, 1, 1, StabilisingForceType.Neutral, 10, 13185.42165, 2.4352493, 0.16832474301, 9.3630680357)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.77, 0, 1, 1, StabilisingForceType.Neutral, 5, 1089583.124, 0.80412429, 0.76200607, 0.229957211)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.0, 1.0, 1, 1, StabilisingForceType.Destabilising, 5, 360177.88981764781, 1.3986042, 0.4623695, 0.3789799)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.0, 1.0, 1, 1, StabilisingForceType.Stabilising, 5, 1052100.32, 0.81832306, 0.75468245, 0.23218877)]

        public async Task SteelLateralBucklingDefault_Resistance(string sectionType, string sectionName, string materialName, double Med, double coef, double c1, double c2, double kw, double kz, StabilisingForceType stabilisingForce, double length, double mcr, double lambda, double chi, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateLateralBucklingInput(sec!, mat!, Med, length, coef, kw, kz, c1, c2, LateralBucklingType.Default, stabilisingForce);
            var lateralBuckling = SteelLateralBucklingFactory.Create(input);

            // Assert
            Assert.Equal(mcr, lateralBuckling.Mcr, 0.1);
            Assert.Equal(lambda, lateralBuckling.LambdaBar, 0.1);
            Assert.Equal(chi, lateralBuckling.Chi, 1e-4);
            Assert.Equal(ratio, lateralBuckling.Ratio, 1e-4);
        }

        [Theory]
        [InlineData("IPE", "IPE200", "S355", 123456, 1, 1, 1, 1, 1, StabilisingForceType.Neutral, 10, 13185.42165, 2.4352493, 0.16832474301, 9.3630680357)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.77, 0, 1, 1, StabilisingForceType.Neutral, 5, 1089583.124, 0.80412429, 0.870041535, 0.201402789)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.77, 1.0, 1, 1, StabilisingForceType.Destabilising, 5, 637514.86497723, 1.05125, 0.7069274, 0.24787379)]
        [InlineData("HEA", "HEA400", "S275", 123456, 2, 1.77, 1.0, 1, 1, StabilisingForceType.Stabilising, 5, 1862217.5731298, 0.615089, 0.9658789, 0.181419004)]

        public async Task SteelLateralBucklingCorrected_Resistance(string sectionType, string sectionName, string materialName, double Med, double coef, double c1, double c2, double kw, double kz, StabilisingForceType stabilisingForce, double length, double mcr, double lambda, double chi, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateLateralBucklingInput(sec!, mat!, Med, length, coef, kw, kz, c1, c2, LateralBucklingType.CorrectedLateralBuckling, stabilisingForce);
            var lateralBuckling = SteelLateralBucklingFactory.Create(input);

            // Assert
            Assert.Equal(mcr, lateralBuckling.Mcr, 0.1);
            Assert.Equal(lambda, lateralBuckling.LambdaBar, 0.1);
            Assert.Equal(chi, lateralBuckling.Chi, 1e-4);
            Assert.Equal(chi, lateralBuckling.Chi, 1e-4);
            Assert.Equal(ratio, lateralBuckling.Ratio, 1e-4);
        }
    }

}
