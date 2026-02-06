using Ofel.Core.StructureDesign.Eurocode3;
using Ofel.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Tests.StructureDesign.Eurocode3
{

    public class SteelShearTubeTests
    {
        [Fact]
        public async Task SteelShear_Y_TCARTube100x4()
        {
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S235", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("TCAR", "TCAR100x4");

            var effort = new ForceValue(0, -50000, 0);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);
            var torsionInput = new SteelTorsionInput
            {
                Context = context
            };
            var torsion = new SteelTorsion_Tube(torsionInput);

            var input = new SteelShearInput
            {
                Context = context,
                Axis = Axis.Y,
                Torsion = torsion
            };

            // Act
            var shear = new SteelShear_Y_Tube(input);

            // Assert
            Assert.Equal(Axis.Y, shear.Axis);
            Assert.Equal(101405.22, shear.VRd, 1.0);
            Assert.Equal(0.493071245, shear.Ratio, 1e-4);
            Assert.Equal(0.0, shear.Rho, 1e-6);
        }

        [Fact]
        public async Task SteelShear_Z_TCARTube100x4()
        {
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S275", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("TCAR", "TCAR80x4");

            var effort = new ForceValue(0, 0, -50000);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);
            var torsionInput = new SteelTorsionInput
            {
                Context = context
            };
            var torsion = new SteelTorsion_Tube(torsionInput);

            var input = new SteelShearInput
            {
                Context = context,
                Axis = Axis.Z,
                Torsion = torsion
            };

            // Act
            var shear = new SteelShear_Z_Tube(input);

            // Assert
            Assert.Equal(Axis.Z, shear.Axis);
            Assert.Equal(93262.28, shear.VRd, 1.0);
            Assert.Equal(0.536122453, shear.Ratio, 1e-4);
            Assert.Equal(0.005219326, shear.Rho, 1e-6);
        }

        [Fact]
        public async Task SteelShear_Y_IPE330()
        {
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S275", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("IPE", "IPE330");

            var effort = new ForceValue(0, 223455, 0, 1234);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);
            var torsionInput = new SteelTorsionInput
            {
                Context = context
            };
            var torsion = new SteelTorsion_IPEHEA(torsionInput);

            var input = new SteelShearInput
            {
                Context = context,
                Axis = Axis.Y,
                Torsion = torsion
            };

            // Act
            var shear = new SteelShear_Y_IPEHEA(input);

            // Assert
            Assert.Equal(Axis.Y, shear.Axis);
            Assert.Equal(579984.839764, shear.VRd, 0.01);
            Assert.Equal(0.3852773, shear.Ratio, 1e-4);
            Assert.Equal(0.00000, shear.Rho, 1e-6);
        }


        [Fact]
        public async Task SteelShear_Z_IPE330()
        {
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S275", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("IPE", "IPE330");

            var effort = new ForceValue(0, 0, -12131, -1234);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);
            var torsionInput = new SteelTorsionInput
            {
                Context = context
            };
            var torsion = new SteelTorsion_IPEHEA(torsionInput);

            var input = new SteelShearInput
            {
                Context = context,
                Axis = Axis.Z,
                Torsion = torsion
            };

            // Act
            var shear = new SteelShear_Z_IPEHEA(input);

            // Assert
            Assert.Equal(Axis.Z, shear.Axis);
            Assert.Equal(446882.73220296, shear.VRd, 0.01);
            Assert.Equal(0.02714582, shear.Ratio, 1e-4);
            Assert.Equal(0.00000, shear.Rho, 1e-6);
            Assert.Equal(1.0, shear.ChiV);
        }
    }
}
