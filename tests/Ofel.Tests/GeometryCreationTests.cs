using System.Collections.Generic;
using System.Linq;
using Xunit;
using ofel.Core;

namespace Ofel.Tests
{
    public class GeometryCreationTests : BaseTestWithResults
    {
        [Fact]
        public void SteelSection_LoadFromCsv_Contains_IPE200()
        {
            // find data file relative to test assembly output
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

            var csvPath = FindData("steel_section.csv");
            var all = SteelSection.LoadFromCsv(csvPath);

            // Act
            var sec = SteelSection.GetByTypeAndName("IPE", "IPE200", csvPath);

            // Assert
            Assert.NotNull(all);
            Assert.NotEmpty(all);
            Assert.NotNull(sec);
            Assert.Equal("IPE", sec.ProfileType);
            Assert.Equal("IPE200", sec.Name);

            // write a short summary to the centralized results file
            var expected = new List<string> { "IPE200 present in steel_section.csv" };
            var calculated = new List<string> { $"Found: {sec?.Name ?? "null"}", $"TotalSections: {all?.Count() ?? 0}" };
            WriteResults(expected, calculated, nameof(SteelSection_LoadFromCsv_Contains_IPE200));
        }
    }
}
