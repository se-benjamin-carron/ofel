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
    public class BucklingTests
    {
        private static SteelBucklingDefaultInput CreateBucklingInput(
            SteelSection section,
            SteelMaterial material,
            double Ned,
            double length,
            double coefBuckling,
            Axis axis)
        {
            var effort = new ForceValue(Ned, 0, 0, 0, 0, 0);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());
            var intervalBuckling = new IntervalBuckling { K = coefBuckling, Start = 0, End = 1, Length = length };
            return new SteelBucklingDefaultInput { Axis = axis, Context = context, Interval = intervalBuckling };
        }
        private static SteelBucklingTorsionInput CreateBucklingTorsionInput(
            SteelSection section,
            SteelMaterial material,
            double Ned,
            double length,
            double ky,
            double kz,
            double kt)
        {
            var effort = new ForceValue(Ned, 0, 0, 0, 0, 0);

            var context = new SteelVerificationContext(effort, section, material, new SteelResistanceCoefficient());
            var buckYInput = new SteelBucklingDefaultInput() { Context = context, Axis = Axis.Y, Interval = new IntervalBuckling() { Start = 0, End = 1, K = ky, Length = length } };
            var buckY = SteelBucklingFactory.Create(buckYInput);
            var buckZInput = new SteelBucklingDefaultInput() { Context = context, Axis = Axis.Z, Interval = new IntervalBuckling() { Start = 0, End = 1, K = kz, Length = length } };
            var buckZ = SteelBucklingFactory.Create(buckZInput);

            var intervalBuckling = new IntervalBuckling { K = kt, Start = 0, End = 1, Length = length };
            return new SteelBucklingTorsionInput { Context = context, BucklingY = buckY, BucklingZ = buckZ, Interval = intervalBuckling };
        }

        [Theory]
        [InlineData("IPE", "IPE200", "S355", 123456, 1, 10.0, Axis.Y, 402087.6833, 1.585824205, 0.3384390188, 0.36074549)]
        [InlineData("IPE", "IPE200", "S355", 123456, 1, 10.0, Axis.Z, 29431.160324, 5.8615400, 0.02752012, 4.43640303)]
        [InlineData("TCAR", "TCAR120x4", "S235", 123456, 2, 10.0, Axis.Z, 20829.8000, 4.524866, 0.04406975, 6.568631)]
        [InlineData("TCAR", "TCAR120x4", "S235", 123456, 0.7, 10.0, Axis.Y, 170039.1843959, 1.5837032, 0.28888862, 1.0020485)]

        public async Task SteelBucklingDefault_Resistance(string sectionType, string sectionName, string materialName, double Ned, double coef, double length, Axis axis, double ncr, double lambda, double chi, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateBucklingInput(sec!, mat!, Ned, length, coef, axis);
            var buckling = new SteelBucklingDefault(input);

            // Assert
            Assert.Equal(ncr, buckling.Ncr, 0.1);
            Assert.Equal(lambda, buckling.LambdaBar, 0.1);
            Assert.Equal(chi, buckling.Chi, 1e-4);
            Assert.Equal(ratio, buckling.Ratio, 1e-4);
        }


        [Theory]
        [InlineData("IPE", "IPE200", "S355", 123456, 10.0, 1, 1, 1, 808169.4641, 402087.683300, 29431.160324048, 29431.160324, 4.4364030348)]
        public async Task SteelBucklingTorsion_Resistance(string sectionType, string sectionName, string materialName, double Ned, double length,
            double ky, double kz, double kt, double ncrt, double ncrtfy, double ncrtfz, double ncr, double ratio)
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync(materialName, "NF EN 10025-2");
            var sec = await repo.GetSectionAsync(sectionType, sectionName);

            var input = CreateBucklingTorsionInput(sec!, mat!, Ned, length, ky, kz, kt);
            var buckling = new SteelBucklingTorsion(input);

            // Assert

            Assert.Equal(ncrt, buckling.Ncrt, 1e-4);
            Assert.Equal(ncrtfz, buckling.NcrTfz, 1e-6);
            Assert.Equal(ncrtfy, buckling.NcrTfy, 1e-6);
            Assert.Equal(ncr, buckling.Ncr, 1e-4);

            Assert.Equal(ratio, buckling.Ratio, 1e-4);
        }
    }

}
