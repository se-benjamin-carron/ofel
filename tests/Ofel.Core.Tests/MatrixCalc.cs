using MathNet.Numerics.LinearAlgebra;
using Ofel.Core.SectionParameter;
using Ofel.MatrixCalc;
using System;
using System.Collections.Generic;
using Xunit;

namespace Ofel.Core.Tests
{
    public class MassMatrixTests
    {
        const double TOL = 1e-9;

        [Fact]
        public void MassMatrix_BasicEntries()
        {
            // rho, S, l, Jx
            var rho = 1.0;
            var S = 1.0;
            var l = 1.0;
            var Jx = 2.0;

            var M = MassMatrixClass.GetMassMatrix(rho, S, l, Jx);

            Assert.Equal(12, M.RowCount);
            Assert.Equal(12, M.ColumnCount);

            // Check a few known formula-based entries (without global scaling they equal the constants)
            Assert.Equal(1.0 / 3.0 * rho * S * l, M[0, 0], TOL);
            Assert.Equal(1.0 / 6.0 * rho * S * l, M[0, 6], TOL);
            Assert.Equal(13.0 / 35.0 * rho * S * l, M[1, 1], TOL);
            Assert.Equal(Jx / (3.0 * S) * rho * S * l, M[3, 3], TOL);

            // Write a short summary to tests results
            var expected = new List<string> { "MassMatrix basic entries" };
            var calculated = new List<string> { $"M[0,0]={M[0, 0]:G17}", $"M[3,3]={M[3, 3]:G17}" };
        }

        [Fact]
        public void MassMatrix_IsSymmetric()
        {
            var rho = 2.3;
            var S = 0.75;
            var l = 1.37;
            var Jx = 0.12;

            var M = MassMatrixClass.GetMassMatrix(rho, S, l, Jx);

            // symmetric matrix M == M^T
            for (int i = 0; i < M.RowCount; i++)
                for (int j = 0; j < M.ColumnCount; j++)
                    Assert.Equal(M[i, j], M[j, i], 1e-12);
        }

        [Fact]
        public void MassMatrix_Formulas_MatchExpected()
        {
            // choose arbitrary parameters and verify specific entries computed from formulas
            var rho = 2.0;
            var S = 3.0;
            var l = 0.5;
            var Jx = 0.08;

            var M = MassMatrixClass.GetMassMatrix(rho, S, l, Jx);

            // formula examples
            // M[4,4] = (l*l/105.0) * (rho*S*l) = rho*S*l^3 / 105
            var expected_44 = rho * S * Math.Pow(l, 3) / 105.0;
            Assert.Equal(expected_44, M[4, 4], 1e-12);

            // M[1,11] = (-13/420 * l) * (rho*S*l) = -13/420 * rho*S*l^2
            var expected_1_11 = -13.0 / 420.0 * rho * S * l * l;
            Assert.Equal(expected_1_11, M[1, 11], 1e-12);

            // M[3,9] = (Jx /(6*S)) * (rho*S*l) = Jx * rho * l / 6
            var expected_3_9 = Jx * rho * l / 6.0;
            Assert.Equal(expected_3_9, M[3, 9], 1e-12);
        }

        [Fact]
        public void MassMatrix_FullMatrixMatchesExpected()
        {
            // Use simple parameters so expected values are easy to reason about
            var rho = 1.5;
            var S = 2.0;
            var l = 1.2;
            var Jx = 0.25;

            var M = MassMatrixClass.GetMassMatrix(rho, S, l, Jx);

            // Build expected matrix using the same formula as implementation
            var E = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(12, 12);

            E[0, 0] = 1.0 / 3.0; E[0, 6] = 1.0 / 6.0;
            E[1, 1] = 13.0 / 35.0; E[1, 7] = 9.0 / 70.0;
            E[2, 2] = 13.0 / 35.0; E[2, 8] = 9.0 / 70.0;
            E[3, 3] = Jx / (3.0 * S); E[3, 9] = Jx / (6.0 * S);
            E[4, 4] = l * l / 105.0; E[4, 10] = l * l / 140.0;
            E[5, 5] = l * l / 105.0; E[5, 11] = l * l / 140.0;
            E[6, 0] = 1.0 / 6.0; E[6, 6] = 1.0 / 3.0;
            E[7, 1] = 9.0 / 70.0; E[7, 7] = 13.0 / 35.0;
            E[8, 2] = 9.0 / 70.0; E[8, 8] = 13.0 / 35.0;
            E[9, 3] = Jx / (6.0 * S); E[9, 9] = Jx / (3.0 * S);
            E[10, 4] = l * l / 140.0; E[10, 10] = l * l / 105.0;
            E[11, 5] = l * l / 140.0; E[11, 11] = l * l / 105.0;

            E[1, 5] = 11.0 * l / 210.0; E[1, 11] = -13.0 * l / 420.0;
            E[2, 4] = -11.0 * l / 210.0; E[2, 10] = 13.0 * l / 420.0;
            E[4, 2] = -11.0 * l / 210.0; E[4, 8] = 13.0 * l / 420.0;
            E[5, 1] = 11.0 * l / 210.0; E[5, 7] = -13.0 * l / 420.0;

            // Mirror symmetric terms
            E[5, 1] = E[1, 5]; E[11, 1] = E[1, 11];
            E[4, 2] = E[2, 4]; E[10, 2] = E[2, 10];
            E[4, 8] = E[8, 4]; E[10, 8] = E[8, 10];
            E[5, 7] = E[7, 5]; E[11, 7] = E[7, 11];

            // Scale
            E *= rho * S * l;

            // Compare full matrices element-wise
            double maxDiff = 0.0;
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    var a = M[i, j];
                    var b = E[i, j];
                    var diff = Math.Abs(a - b);
                    if (diff > maxDiff) maxDiff = diff;
                    Assert.Equal(b, a, 1e-12);
                }
            }
        }
        [Fact]
        public void TestsStiffnessNormal()
        {
            var section = new SteelSection("IPE", "IPE100", 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0);
            var material = new SteelMaterial("S275", "NF EN 10025-2", 275e6, 420e6, 210000e6, 81000e6, 7850, 1);
            var left = new IsHinged(false, false, false);
            var right = new IsHinged(false, false, false);

            // Act
            Matrix<double> result = MatrixHelpers.Stiffness(section, material, left, right, 1.0);
            Assert.NotNull(result);
            Assert.True(result.RowCount > 0 && result.ColumnCount > 0);
            Assert.Equal(result.RowCount, result.ColumnCount); // matrice carrée attendue
        }
    }
}
