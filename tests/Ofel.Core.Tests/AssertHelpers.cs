using MathNet.Numerics.LinearAlgebra;
using Xunit;

namespace Ofel.Core.Tests;

public static class AssertHelpers
{
    public static void AssertVectorEqual(
        IReadOnlyList<double> expected,
        Vector<double> actual,
        int precision = 3)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (int i = 0; i < expected.Count; i++)
            Assert.Equal(expected[i], actual[i], precision);
    }

    public static void AssertSupportReactionsEqual(
        Dictionary<string, Dictionary<int, double[]>> expected,
        Dictionary<string, Dictionary<int, Vector<double>>> actual,
        int precision = 3)
    {
        foreach (var (loadCase, expectedBySupport) in expected)
        {
            Assert.True(actual.ContainsKey(loadCase),
                $"Missing load case '{loadCase}'");

            var actualBySupport = actual[loadCase];

            foreach (var (supportId, expectedVector) in expectedBySupport)
            {
                Assert.True(actualBySupport.ContainsKey(supportId),
                    $"Missing support {supportId} in load case '{loadCase}'");

                AssertVectorEqual(
                    expectedVector,
                    actualBySupport[supportId],
                    precision);
            }
        }
    }
}
