using Xunit;
using ofel.Core;
using Ofel.MatrixCalc;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Ofel.Tests
{
    public class MatrixCalcTests
    {
        const double TOL = 1e-9;

        [Fact]
        [Trait("Category", "Unit")]
        public void Normal_MatrixHasExpectedValuesAndSymmetry()
        {
            double E = 200.0;
            double A = 2.0;
            double L = 4.0;
            var K = MatrixCalc.Normal(E, A, L); // expected k = E*A/L = 100

            double expected = E * A / L;
            Assert.Equal(2, K.RowCount);
            Assert.Equal(2, K.ColumnCount);
            Assert.InRange(K[0, 0], expected - TOL, expected + TOL);
            Assert.InRange(K[1, 1], expected - TOL, expected + TOL);
            Assert.InRange(K[0, 1], -expected - TOL, -expected + TOL);
            Assert.InRange(K[1, 0], -expected - TOL, -expected + TOL);
            // symmetry
            Assert.Equal(K[0,1], K[1,0], TOL);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TorsionFixed_CorrectValuesAndSymmetry()
        {
            double G = 80.0;
            double It = 2.0;
            double L = 4.0;
            var K = MatrixCalc.TorsionFixed(G, It, L); // k = G*It/L = 40

            double expected = G * It / L;
            Assert.Equal(2, K.RowCount);
            Assert.Equal(2, K.ColumnCount);
            Assert.InRange(K[0, 0], expected - TOL, expected + TOL);
            Assert.InRange(K[1, 1], expected - TOL, expected + TOL);
            Assert.InRange(K[0, 1], -expected - TOL, -expected + TOL);
            Assert.InRange(K[1, 0], -expected - TOL, -expected + TOL);
            Assert.Equal(K[0,1], K[1,0], TOL);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void FlexionFixed_BasicNumericCheck_And_Symmetry()
        {
            double E = 2000.0;
            double G = 800.0;
            double I = 10.0;
            double A_v = 1.0;
            double L = 2.0;
            bool isYAxis = true;
            var K = MatrixCalc.FlexionFixed(E, G, I, A_v, L, isYAxis);

            // basic structural checks
            Assert.Equal(4, K.RowCount);
            Assert.Equal(4, K.ColumnCount);

            // compute expected scalar used in K[0,0] per formula
            double phi = 12.0 * E * I / (G * A_v * L * L);
            double expected00 = 12.0 * E * I / (Math.Pow(L, 3) * (1 + phi));
            double expected01 = (6.0 * E * I) / (Math.Pow(L, 2) * (1 + phi));
            double coef = isYAxis ? 1.0 : -1.0;

            Assert.InRange(K[0,0], expected00 - 1e-6, expected00 + 1e-6);
            Assert.InRange(K[0,1], coef * expected01 - 1e-6, coef * expected01 + 1e-6);
            // symmetry and sign relations
            Assert.Equal(K[0,2], -K[0,0], 1e-9);
            Assert.Equal(K[0,1], coef * K[1,0], 1e-9);
            Assert.Equal(K[2,0], K[0,2], 1e-9);
            Assert.Equal(K[3,3], K[1,1], 1e-9);
        }
    }
}