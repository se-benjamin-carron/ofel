using MathNet.Numerics.LinearAlgebra;

namespace Ofel.MatrixCalc
{
    public static class MassMatrixClass
    {
        public static Matrix<double> GetMassMatrix(double rho, double S, double l, double Jx)
        {
            var M = Matrix<double>.Build.Dense(12, 12);
            // Row 0
            M[0, 0] = 1.0 / 3.0; M[0, 6] = 1.0 / 6.0;
            // Row 1
            M[1, 1] = 13.0 / 35.0; M[1, 7] = 9.0 / 70.0;
            // Row 2
            M[2, 2] = 13.0 / 35.0; M[2, 8] = 9.0 / 70.0;
            // Row 3
            M[3, 3] = Jx / (3.0 * S); M[3, 9] = Jx / (6.0 * S);
            // Row 4
            M[4, 4] = l * l / 105.0; M[4, 10] = l * l / 140.0;
            // Row 5
            M[5, 5] = l * l / 105.0; M[5, 11] = l * l / 140.0;
            // Row 6
            M[6, 0] = 1.0 / 6.0; M[6, 6] = 1.0 / 3.0;
            // Row 7
            M[7, 1] = 9.0 / 70.0; M[7, 7] = 13.0 / 35.0;
            // Row 8
            M[8, 2] = 9.0 / 70.0; M[8, 8] = 13.0 / 35.0;
            // Row 9
            M[9, 3] = Jx / (6.0 * S); M[9, 9] = Jx / (3.0 * S);
            // Row 10
            M[10, 4] = l * l / 140.0; M[10, 10] = l * l / 105.0;
            // Row 11
            M[11, 5] = l * l / 140.0; M[11, 11] = l * l / 105.0;

            // Off-diagonal symmetric terms
            M[1, 5] = 11.0 * l / 210.0; M[1, 11] = -13.0 * l / 420.0;
            M[2, 4] = -11.0 * l / 210.0; M[2, 10] = 13.0 * l / 420.0;
            M[4, 2] = -11.0 * l / 210.0; M[4, 8] = 13.0 * l / 420.0;
            M[5, 1] = 11.0 * l / 210.0; M[5, 7] = -13.0 * l / 420.0;

            // Mirror symmetric terms
            M[5, 1] = M[1, 5]; M[11, 1] = M[1, 11];
            M[4, 2] = M[2, 4]; M[10, 2] = M[2, 10];
            M[4, 8] = M[8, 4]; M[10, 8] = M[8, 10];
            M[5, 7] = M[7, 5]; M[11, 7] = M[7, 11];

            // Scale by rho * S * l
            M *= rho * S * l;
            return M;
        }
    }
}