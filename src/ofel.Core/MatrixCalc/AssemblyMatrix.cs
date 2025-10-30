using MathNet.Numerics.LinearAlgebra;

namespace Ofel.MatrixCalc
{
    class AssemblyMatrixClass
    {
        // Assemble local matrix/vector into global coordinates:
        // K_global = A * K_local * A^T
        public static Matrix<double> AssembleMatrix(Matrix<double> assemblyMatrix, Matrix<double> MatrixToAssemble)
        {
            return assemblyMatrix.Multiply(MatrixToAssemble).Multiply(assemblyMatrix.Transpose());
        }

        // Assemble local vector into global coordinates: f_global = A * f_local
        public static Vector<double> AssembleVector(Matrix<double> assemblyMatrix, Vector<double> vectorToAssemble)
        {
            return assemblyMatrix.Multiply(vectorToAssemble);
        }

        // Disassemble global matrix to local coordinates: K_local = A^T * K_global * A
        public static Matrix<double> DisAssembleMatrix(Matrix<double> assemblyMatrix, Matrix<double> MatrixToDisAssemble)
        {
            return assemblyMatrix.TransposeThisAndMultiply(MatrixToDisAssemble).Multiply(assemblyMatrix);
        }

        // Disassemble global vector to local coordinates: f_local = A^T * f_global
        public static Vector<double> DisAssembleVector(Matrix<double> assemblyMatrix, Vector<double> vectorToDisAssemble)
        {
            return assemblyMatrix.TransposeThisAndMultiply(vectorToDisAssemble);
        }

        public static Matrix<double> GetAssemblyMatrix(int size, List<int> connectivity)
        {
            int size_connectivity = connectivity.Count;
            var K = Matrix<double>.Build.Dense(size, size_connectivity);
            for (int i = 0; i < size_connectivity; i++)
            {
                K[connectivity[i], i] = 1.0;
            }
            return K;
        }
    }
}