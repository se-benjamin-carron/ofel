using MathNet.Numerics.LinearAlgebra;

namespace Ofel.MatrixCalc
{
    public static class UniformStressMatrixClass
    {
        public static Matrix<double> GetUniformStressMatrix(double L)
        {
            var K = Matrix<double>.Build.Dense(12, 12);
            // Remplissage complet selon la matrice de la photo
            // Ligne 1
            K[1, 1] = 6.0 / 5.0; K[1, 5] = L / 10.0; K[1, 7] = -6.0 / 5.0; K[1, 11] = L / 10.0;
            K[2, 2] = 6.0 / 5.0; K[2, 4] = L / 10.0; K[2, 8] = -6.0 / 5.0; K[2, 10] = L / 10.0;
            // Ligne 2
            K[4, 2] = L / 10.0; K[4, 4] = 2.0 * L * L / 15.0; K[4, 8] = -L / 10.0; K[4, 10] = L * L / 30.0;
            K[5, 1] = L / 10.0; K[5, 5] = 2.0 * L * L / 15.0; K[5, 7] = -L / 10.0; K[5, 11] = L * L / 30.0;


            K[7, 1] = -6.0 / 5.0; K[7, 5] = -L / 10.0; K[7, 7] = 6.0 / 5.0; K[7, 11] = -L / 10.0;
            K[8, 2] = -6.0 / 5.0; K[8, 4] = -L / 10.0; K[8, 8] = 6.0 / 5.0; K[8, 10] = -L / 10.0;
            K[10, 1] = L / 10.0; K[10, 5] = -L * L / 30.0; K[10, 7] = -L / 10.0; K[10, 11] = 2.0 * L * L / 15.0;
            K[11, 2] = L / 10.0; K[11, 4] = -L * L / 30.0; K[11, 8] = -L / 10.0; K[11, 10] = 2.0 * L * L / 15.0;

            K *= 1 / L;
            return K;
        }
    }
}
