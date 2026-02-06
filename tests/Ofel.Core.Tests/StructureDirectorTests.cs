using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Ofel.Core;
using Xunit;

namespace Ofel.Core.Tests;

public class StructureDirectorTests
{
    [Fact]
    public void LinearForceOnBeam_ComputesExpectedReactions()
    {
        var structure = StructureDirector.CreateBeamLinearForce(span: 5.0, height: 2.0);
        structure.Prepare(mesh: 0.1);
        structure.Compute();

        var expected = new Dictionary<string, Dictionary<int, double[]>>
        {
            ["Wind_1"] = new()
            {
                [13] = new[] { 0.0, 0.0, 13462.91233598, 0.0, 0.0, 0.0 },
                [31] = new[] { 0.0, 0.0, 13462.91169969, 0.0, 0.0, 0.0 }
            },
            ["Collision_1"] = new()
            {
                [13] = new[] { 4687.184449219, 0.0, 12620.63037022, 0.0, 0.0, 0.0 },
                [31] = new[] { 4687.18421639, 0.0, 12620.6297433, 0.0, 0.0, 0.0 }
            },
            ["OwnWeight_1"] = new()
            {
                [13] = new[] { 0.0, 0.0, 586.8608, 0.0, 0.0, 0.0 },
                [31] = new[] { 0.0, 0.0, 586.8608, 0.0, 0.0, 0.0 }
            }
        };
        var Vectors = structure.SupportReactions["Wind_1"];
        AssertHelpers.AssertSupportReactionsEqual(expected, structure.SupportReactions, precision: 2);
    }

    [Fact]
    public void LocalForce_ComputesExpectedReactions()
    {
        var structure = StructureDirector.CreateHingedBeam(length: 5.0);
        structure.Prepare(mesh: 1.0);
        structure.Compute();

        var expected = new Dictionary<string, Dictionary<int, double[]>>
        {
            ["Wind_1"] = new()
            {
                [0] = new[] { 0.0, -625.001272603182, -250.000506626, 0.0, 0.0, 0.0 },
                [12] = new[] { 0.0, -1874.99876, -749.9994933, 0.0, 0.0, 0.0 }
            },
            ["Collision_1"] = new()
            {
                [0] = new[] { -2500.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
                [12] = new[] { -2500.0, 0.0, 0.0, 0.0, 0.0, 0.0 }
            }
        };
        var Vectors = structure.SupportReactions["Wind_1"];
        AssertHelpers.AssertSupportReactionsEqual(expected, structure.SupportReactions, precision: 2);
    }

    [Fact]
    public void CantileverBeam_ComputesExpectedReaction()
    {
        var structure = StructureDirector.CreateCantileverBeam(length: 3.0);
        structure.Prepare(mesh: 0.8);
        structure.Compute();

        var expected = new[] { 0.0, 0.0, -10000.0, 0.0, 30000.0, 0.0 };
        var actual = structure.SupportReactions["Wind_1"].Values.Single();

        AssertHelpers.AssertVectorEqual(expected, actual, precision: 3);
    }

    [Fact]
    public void DefaultRoof_ComputesExpectedSupportReactions()
    {
        var structure = StructureDirector.CreateDefaultRoof(span: 5.0, height: 2.0);
        structure.Prepare(mesh: 1.0);
        structure.Compute();

        var expected = new Dictionary<string, Dictionary<int, double[]>>
        {
            ["Wind_1"] = new()
            {
                [0] = new[] { -4223.8876, 0.0, -2656.17376, 0.0, 0.0, 0.0 },
                [10] = new[] { 3723.8879, 0.0, -2343.8262, 0.0, 0.0, 0.0 }
            },
            ["Collision_1"] = new()
            {
                [0] = new[] { 3973.8876, 0.0, 2500.0, 0.0, 0.0, 0.0 },
                [10] = new[] { -3973.8876, 0.0, 2500.0, 0.0, 0.0, 0.0 }
            }
        };
        AssertHelpers.AssertSupportReactionsEqual(expected, structure.SupportReactions);
    }
}
