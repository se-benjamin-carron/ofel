using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Tests.StructureDesign.Eurocode3
{
    using Ofel.Core.Data;
    using Ofel.Core.StructureDesign.Eurocode3;
    using Ofel.Infrastructure.Data;
    using Ofel.Infrastructure.Utils;
    using Xunit;

    public class SteelTorsionTests
    {
        [Fact]
        public async Task Torsion_TCARTube100x4()
        {
            // Arrange : DB
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S235", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("TCAR", "TCAR100x4");

            Assert.NotNull(material);
            Assert.NotNull(section);

            var effort = new ForceValue(0, 0, 0, 12000, 0, 0);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);

            var input = new SteelTorsionInput
            {
                Context = context
            };

            // Act
            var torsion = new SteelTorsion_Tube(input);

            // Assert
            Assert.Equal(150e6, torsion.Tau_t, 1e-6);
            Assert.Equal(1.105564345, torsion.Ratio, 1e-6);
        }
        [Fact]
        public async Task Torsion_HEA400()
        {
            // Arrange : DB
            var repo = await OfelDbFactory.CreateFullDbFromCsvAsync();

            var material = await repo.GetMaterialAsync("S275", "NF EN 10025-2");
            var section = await repo.GetSectionAsync("HEA", "HEA400");

            Assert.NotNull(material);
            Assert.NotNull(section);

            var effort = new ForceValue(0, 0, 0, 12345, 0, 0);

            var coef = new SteelResistanceCoefficient();

            var context = new SteelVerificationContext(effort, section!, material!, coef);

            var input = new SteelTorsionInput
            {
                Context = context
            };

            // Act
            var torsion = new SteelTorsion_IPEHEA(input);

            // Assert
            Assert.Equal(71849206.35, torsion.Tau_w, 1.0);
            Assert.Equal(124103174.60, torsion.Tau_f, 1.0);
            Assert.Equal(0.7816472865, torsion.Ratio, 1e-4);

        }
    }

}
