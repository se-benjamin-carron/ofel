using MathNet.Numerics.LinearAlgebra;
using System.Drawing;

namespace Ofel.MatrixCalc
{
    class AssemblyMatrixClass
    {
        // Assemble local matrix/vector into global coordinates:
        // K_global = A * K_local * A^T
        public static Matrix<double> AssembleMatrix(Matrix<double> assemblyMatrix, Matrix<double> MatrixToAssemble)
        {
            return assemblyMatrix.Transpose().Multiply(MatrixToAssemble).Multiply(assemblyMatrix);
        }

        // Assemble local vector into global coordinates: f_global = A * f_local
        public static Vector<double> AssembleVector(Matrix<double> assemblyMatrix, Vector<double> vectorToAssemble)
        {
            return assemblyMatrix.Multiply(vectorToAssemble);
        }

        // Disassemble global matrix to local coordinates: K_local = A^T * K_global * A
        public static Matrix<double> DisAssembleMatrix(Matrix<double> assemblyMatrix, Matrix<double> MatrixToDisAssemble)
        {
            var assemblyMatrixTranspose = assemblyMatrix.Transpose();
            return assemblyMatrix.Multiply(MatrixToDisAssemble).Multiply(assemblyMatrixTranspose);
        }

        // Disassemble global vector to local coordinates: f_local = A^T * f_global
        public static Vector<double> DisAssembleVector(Matrix<double> assemblyMatrix, Vector<double> vectorToDisAssemble)
        {
            return assemblyMatrix.Multiply(vectorToDisAssemble);
        }

        public static Matrix<double> GetAssemblyMatrix(int size, List<int> connectivity)
        {
            int size_connectivity = connectivity.Count;
            var K = Matrix<double>.Build.Dense(size_connectivity, size);
            for (int i = 0; i < size_connectivity; i++)
            {
                K[i, connectivity[i]] = 1.0;
            }
            return K;
        }
        public static Matrix<double> GetAssemblyMatrixSupport(int size, List<int> connectivity, int sizeSupport = 6)
        {

            int size_connectivity = connectivity.Count;
            var K = Matrix<double>.Build.Dense(6, size);
            for (int i = 0; i < size_connectivity; i++)
            {
                int rowIndex = connectivity[i] % sizeSupport;
                K[rowIndex, connectivity[i]] = 1.0;
            }
            return K;
        }
    }
}
