using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Tests
{
    public class VariableRepartitionTests
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

            var expected = new List<VariableRepartition>
            {
                // minimal assertions expressed as expected strings for WriteResults
                new VariableRepartition("", new List<string> { }),
                new VariableRepartition("S1", new List<string> { }),
                new VariableRepartition("S1", new List<string> { "Q2"}),
                new VariableRepartition("S1", new List<string> { "W3"}),
                new VariableRepartition("S1", new List<string> { "Q2", "W3"}),
                new VariableRepartition("Q2", new List<string> { }),
                new VariableRepartition("Q2", new List<string> { "S1"}),
                new VariableRepartition("Q2", new List<string> { "W3"}),
                new VariableRepartition("Q2", new List<string> {"S1", "W3"}),
                new VariableRepartition("W3", new List<string> { }),
                new VariableRepartition("W3", new List<string> { "S1"}),
                new VariableRepartition("W3", new List<string> { "Q2"}),
                new VariableRepartition("W3", new List<string> { "S1", "Q2"}),
            };

            Assert.Equal(13, result.Count);
            Assert.Contains(result, r => r.Major == "S1" && r.Minors.Count == 2 && r.Minors.Contains("Q2") && r.Minors.Contains("W3"));
            Assert.Contains(result, r => r.Major == "Q2" && r.Minors.Count == 1 && r.Minors.Contains("W3"));
            Assert.Contains(result, r => r.Major == "W3" && r.Minors.Count == 0);
        }
    }
}
