using System.Collections.Generic;
using System.Linq;
using Xunit;
using ofel.Core;

namespace Ofel.Tests
{
    public class SteelMaterialCreationTests : BaseTestWithResults
    {
        [Fact]
        public void SteelMaterial_LoadFromCsv_Contains_S235()
        {
            string FindData(string f)
            {
                var dir = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);
                while (dir != null)
                {
                    var candidate = System.IO.Path.Combine(dir.FullName, "data", f);
                    if (System.IO.File.Exists(candidate)) return candidate;
                    dir = dir.Parent;
                }
                throw new System.IO.FileNotFoundException(f);
            }

            var csvPath = FindData("steel_material.csv");
            var all = SteelMaterial.LoadFromCsv(csvPath);
            var mat = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2", csvPath);

            Assert.NotNull(all);
            Assert.NotEmpty(all);
            Assert.NotNull(mat);
            Assert.Equal("S235", mat.Name);
            Assert.Equal("NF EN 10025-2", mat.Standard);
            Assert.True(mat.Fy > 0);
            Assert.True(mat.Fu > 0);

            // write a short summary to the centralized results file
            var expected = new List<string> { "S235 present in steel_material.csv" };
            var calculated = new List<string> { $"Found: {mat?.Name ?? "null"}", $"TotalMaterials: {all?.Count() ?? 0}" };
            WriteResults(expected, calculated, nameof(SteelMaterial_LoadFromCsv_Contains_S235));
        }
    }
}
