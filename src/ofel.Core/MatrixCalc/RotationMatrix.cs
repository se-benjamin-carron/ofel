using MathNet.Numerics.LinearAlgebra;

namespace Ofel.MatrixCalc
{
    class RotationMatrixClass
    {
        /// <summary>
        /// Switches the base of a matrix using a rotation matrix R: returns R^T * M * R
        /// </summary>
        public static Matrix<double> SwitchMatrixFromLocalToGlobal(Matrix<double> rotation_matrix, Matrix<double> matrix_to_switch)
        {
            return rotation_matrix.Transpose() * matrix_to_switch * rotation_matrix;
        }
        public static Matrix<double> SwitchMatrixFromGlobalToLocal(Matrix<double> rotation_matrix, Matrix<double> matrix_to_switch)
        {
            return rotation_matrix * matrix_to_switch * rotation_matrix.Transpose();
        }
        public static Vector<double> SwitchVectorFromGlobalToLocal(Matrix<double> rotation_matrix, Vector<double> vector_to_switch)
        {
            return rotation_matrix * vector_to_switch;
        }
        public static Vector<double> SwitchVectorFromLocalToGlobal(Matrix<double> rotation_matrix, Vector<double> vector_to_switch)
        {
            return rotation_matrix.Transpose() * vector_to_switch;
        }

        // Matrice 3x3 sphérique
        public static Matrix<double> SphericalRotation(double theta, double phi, double roll)
        {
            var part_1 = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { Math.Sin(theta)*Math.Cos(phi),  Math.Sin(theta)*Math.Sin(phi),  Math.Cos(theta) },
                { Math.Cos(theta)*Math.Cos(phi),  Math.Cos(theta)*Math.Sin(phi), -Math.Sin(theta) },
                { -Math.Sin(phi),                 Math.Cos(phi),                  0.0             }
            });
            var part_2 = Matrix<double>.Build.DenseOfArray(new double[,] {
                { 1,  0,  0 },
                { 0,  Math.Cos(roll), -Math.Sin(roll) },
                { 0,  Math.Sin(roll), Math.Cos(roll)}
            });
            return part_1 * part_2;
        }

        public static Matrix<double> SphericalRotation6x6(double theta, double phi, double roll)
        {
            var R3 = SphericalRotation(theta, phi, roll);
            var R6 = Matrix<double>.Build.Dense(6, 6);
            R6.SetSubMatrix(0, 3, 0, 3, R3);
            R6.SetSubMatrix(3, 3, 3, 3, R3);
            return R6;
        }

        public static Matrix<double> BuildGlobalRotationMatrix(List<(double theta, double phi)> angles)
        {
            int n = angles.Count;
            int size = 3 * n;
            var M = Matrix<double>.Build.Dense(size, size);

            for (int k = 0; k < n; k++)
            {
                var R = SphericalRotation(angles[k].theta, angles[k].phi, 0.0);
                int offset = 3 * k;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        M[offset + i, offset + j] = R[i, j];
            }

            return M;
        }
        /// <summary>
        /// Build a block-diagonal matrix by repeating the square input matrix `repeat` times along the diagonal.
        /// Example: a 3x3 matrix repeated 2 times produces a 6x6 matrix with the source in top-left and bottom-right.
        /// </summary>
        public static Matrix<double> ExtendGlobalRotationMatrix(Matrix<double> matrix, int repeat)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            if (matrix.RowCount != matrix.ColumnCount) throw new ArgumentException("matrix must be square", nameof(matrix));
            if (repeat < 1) throw new ArgumentOutOfRangeException(nameof(repeat), "repeat must be >= 1");

            int n = matrix.RowCount;
            int size = n * repeat;
            var M = Matrix<double>.Build.Dense(size, size, 0.0);

            for (int block = 0; block < repeat; block++)
            {
                int off = block * n;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        M[off + i, off + j] = matrix[i, j];
                    }
                }
            }

            return M;
        }
    }
}