using System.Collections.Generic;
using Xunit;
using ofel.Core;

namespace Ofel.Tests
{
    // Test class focused on variable repartitions generation behaviour
    public class VariableRepartitionTests : BaseTestWithResults
    {
        [Fact]
        public void GetAllVariableRepartitions_ReturnsExpectedCombinations()
        {
            var variables = new List<string> { "S1", "Q2", "W3" };
            var result = FiniteElementStructure.GetAllVariableRepartitions(variables);

            var calculatedStrings = new List<string>();
            foreach (var repartition in result)
            {
                calculatedStrings.Add($"Major: {repartition.Major}, Minors: [{string.Join(", ", repartition.Minors)}]");
            }

            var expected = new List<string>
            {
                // minimal assertions expressed as expected strings for WriteResults
                "Major: S1, Minors: []",
            };

            // write into centralized TestsResults.txt for CI inspection
            WriteResults(expected, calculatedStrings, nameof(GetAllVariableRepartitions_ReturnsExpectedCombinations));

            Assert.Equal(12, result.Count);
            Assert.Contains(result, r => r.Major == "S1" && r.Minors.Count == 2 && r.Minors.Contains("Q2") && r.Minors.Contains("W3"));
            Assert.Contains(result, r => r.Major == "Q2" && r.Minors.Count == 1 && r.Minors.Contains("W3"));
            Assert.Contains(result, r => r.Major == "W3" && r.Minors.Count == 0);
        }
    }
}