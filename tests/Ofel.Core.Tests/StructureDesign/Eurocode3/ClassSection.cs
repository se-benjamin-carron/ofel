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
    public class IPESectionTests
    {
        private static ClassSectionInput CreateClassSectionInput(
            SteelSection section,
            SteelMaterial material,
            double ned)
        {
            return new ClassSectionInput(section, material, ned);
        }

        [Fact]
        public async Task IPE100UnderModerateLoad()
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync("S235", "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", "IPE100");

            double ned = 20_000; // 20 kN -> Compression +

            var input = CreateClassSectionInput(sec!, mat!, ned);

            // Act
            var cls = new ClassSection_IPEHEA(input);

            // Assert
            Assert.Equal(1, cls.DesignClass);
            Assert.Equal(1, cls.WebClass);
            Assert.Equal(1, cls.FlangeClass);
            Assert.Equal(74.6e-3, cls.HeightWeb);
            Assert.Equal(18.45e-3, cls.HeightFlange);
            Assert.Equal(18.19512195, cls.SlendernessWeb, 1e-6);
            Assert.Equal(3.236842105, cls.SlendernessFlange, 1e-6);
            Assert.Equal(1, cls.Epsilon);
            Assert.Equal(-0.8350651, cls.Psi, 1e-3);
            Assert.Equal(0.6391263699, cls.Alpha, 1e-6);

        }

        [Theory]
        [InlineData(50_000, 0.5162069721, -0.9756112, 1)]
        [InlineData(-200_000, 0.4351721116, -1.097555, 1)]
        [InlineData(800_000, 0.7593115538, -0.6097799, 2)]
        [InlineData(900_000, 0.7917254980, -0.5610024, 3)]
        public async Task DesignClassEvolutionUnderLoad(double ned, double alpha, double psi, double classSection)
        {
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync("S355", "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", "IPE500");

            var input = CreateClassSectionInput(sec!, mat!, ned);
            var cls = new ClassSection_IPEHEA(input);
            Assert.Equal(0.813616513, cls.Epsilon, 1e-6);
            Assert.Equal(classSection, cls.DesignClass);
            Assert.Equal(alpha, cls.Alpha, 1e-6);
            Assert.Equal(psi, cls.Psi, 1e-4);
        }
        [Theory]
        [InlineData(50_000, -0.5162069721, -1.9756112, 3)]
        [InlineData(-200_000, 0.3351721116, 1.097555, 3)]
        [InlineData(800_000, -0.7593115538, 0.6097799, 4)]
        public async Task DesignClassEvolutionUnderLoad_False(double ned, double alpha, double psi, double classSection)
        {
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync("S275", "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("IPE", "IPE500");

            var input = CreateClassSectionInput(sec!, mat!, ned);
            var cls = new ClassSection_IPEHEA(input);
            Assert.NotEqual(0.813616513, cls.Epsilon, 1e-6);
            Assert.NotEqual(classSection, cls.DesignClass);
            Assert.NotEqual(alpha, cls.Alpha, 1e-6);
            Assert.NotEqual(psi, cls.Psi, 1e-4);
        }
    }

    public class TCARSectionTests
    {
        private static ClassSectionInput CreateTubeInput(
            SteelSection section,
            SteelMaterial material,
            double ned)
        {
            return new ClassSectionInput(section, material, ned);
        }

        [Fact]
        public async Task Results_TCAR100x4_ModerateLoad()
        {
            // Arrange
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync("S235", "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("TCAR", "TCAR100x4");

            double ned = 75_000; // 75 kN

            var input = CreateTubeInput(sec!, mat!, ned);

            // Act
            var cls = new ClassSection_Tube(input);

            // Assert
            Assert.Equal(1, cls.DesignClass);
            Assert.Equal(87.33369529, cls.LimitClassHeight.Class4, 1e-6);
            Assert.Equal(51.16722949, cls.LimitClassBase.Class3, 1e-6);
            Assert.Equal(19, cls.SlendernessBase, 1e-6);
            Assert.Equal(19, cls.SlendernessHeight, 1e-6);
            Assert.Equal(0.7624580067, cls.AlphaHeight, 1e-5);
            Assert.Equal(0.7624580067, cls.AlphaBase, 1e-5);
            Assert.Equal(-0.5729878, cls.Psi, 1e-5);


        }
        [Theory]
        [InlineData(50_000, 0.5222293356, -0.9595465, 44, 55.65610237, 1)]
        [InlineData(500_000, 0.7222933561, -0.5954646, 44, 38.4027767, 2)]
        [InlineData(800_000, 0.85566937, -0.3527433, 44, 31.82552642, 3)]
        public async Task Tube_DesignClass_Load_Increases(double ned, double alpha, double psi, double slenderness, double limit2, int classSection)
        {
            ISteelRepository repo = await OfelDbFactory.CreateFullDbFromCsvAsync();
            var mat = await repo.GetMaterialAsync("S355", "NF EN 10025-2");
            var sec = await repo.GetSectionAsync("TCAR", "TCAR300x6");

            var input = CreateTubeInput(sec!, mat!, ned);
            var cls = new ClassSection_Tube(input);

            Assert.Equal(classSection, cls.DesignClass);
            Assert.Equal(limit2, cls.LimitClassHeight.Class2, 1e-6);
            Assert.Equal(slenderness, cls.SlendernessHeight);
            Assert.Equal(alpha, cls.AlphaHeight, 1e-6);
            Assert.Equal(psi, cls.Psi, 1e-5);
        }



    }

}
