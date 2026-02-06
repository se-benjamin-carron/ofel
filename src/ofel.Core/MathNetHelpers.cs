using MathNet.Numerics.LinearAlgebra;

namespace Ofel.Core
{
    public static class MathNetHelpers
    {
        // Helper to convert float[,] to Matrix<float>
        public static Matrix<float> ToMatrix(float[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var mat = Matrix<float>.Build.Dense(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    mat[i, j] = array[i, j];
            return mat;
        }

        // Helper to convert double[,] to Matrix<double>
        public static Matrix<double> ToMatrix(double[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var mat = Matrix<double>.Build.Dense(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    mat[i, j] = array[i, j];
            return mat;
        }
    }
}
