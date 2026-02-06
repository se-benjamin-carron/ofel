using System;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using Ofel.Core;
using Ofel.Core.SectionParameter;



namespace Ofel.MatrixCalc
{
    /// <summary>
    /// Managed wrapper for computing beam element stiffness matrices using Eigen.NET.
    /// </summary>
    public static class MatrixHelpers
    {
        public static Matrix<double> FlexionFixed(double E, double G, double I, double A_v, double L, bool isYAxis)
        {
            double coef;
            if (isYAxis) coef = 1; else coef = -1;
            double phi = 12.0 * E * I / (G * A_v * L * L);
            var K = Matrix<double>.Build.Dense(4, 4);
            K[0, 0] = (12.0 * E * I / (Math.Pow(L, 3) * (1 + phi)));
            K[0, 1] = coef * (6.0 * E * I / (Math.Pow(L, 2) * (1 + phi)));
            K[0, 2] = -K[0, 0];
            K[0, 3] = K[0, 1];
            K[1, 0] = K[0, 1];
            K[1, 1] = (4.0 + phi) * E * I / (L * (1 + phi));
            K[1, 2] = -K[0, 1];
            K[1, 3] = (2.0 - phi) * E * I / (L * (1 + phi));
            K[2, 0] = K[0, 2];
            K[2, 1] = K[1, 2];
            K[2, 2] = K[0, 0];
            K[2, 3] = -K[0, 1];
            K[3, 0] = K[0, 3];
            K[3, 1] = K[1, 3];
            K[3, 2] = K[2, 3];
            K[3, 3] = K[1, 1];
            return K;
        }

        // Fixed-Hinged (simple compilable implementation)
        public static Matrix<double> FlexionFixedHinged(double E, double G, double I, double L, bool isYAxis)
        {
            double coef = isYAxis ? 1.0 : -1.0;
            var K = Matrix<double>.Build.Dense(4, 4, 0.0);
            double k1 = 3.0 * E * I / Math.Pow(L, 3);
            double k2 = 3.0 * E * I / Math.Pow(L, 2);
            double k3 = 3.0 * E * I / L;
            // Symmetric pattern for fixed-hinged (approximation)
            K[0, 0] = k1;
            K[0, 1] = coef * k2;
            K[0, 2] = -k1;
            K[0, 3] = 0.0;
            K[1, 0] = coef * k2;
            K[1, 1] = k3;
            K[1, 2] = -coef * k2;
            K[1, 3] = 0.0;
            K[2, 0] = -k1;
            K[2, 1] = -coef * k2;
            K[2, 2] = k1;
            K[2, 3] = 0.0;
            K[3, 0] = 0.0;
            K[3, 1] = 0.0;
            K[3, 2] = 0.0;
            K[3, 3] = 0.0;
            return K;
        }

        // Hinged-Fixed (simple compilable implementation)
        public static Matrix<double> FlexionHingedFixed(double E, double G, double I, double L, bool isYAxis)
        {
            double coef = isYAxis ? 1.0 : -1.0;
            var K = Matrix<double>.Build.Dense(4, 4, 0.0);
            double k1 = 3.0 * E * I / Math.Pow(L, 3);
            double k2 = 3.0 * E * I / Math.Pow(L, 2);
            double k3 = 3.0 * E * I / L;
            K[0, 0] = k1;
            K[0, 1] = 0.0;
            K[0, 2] = -k1;
            K[0, 3] = -coef * k2;
            K[1, 0] = 0.0;
            K[1, 1] = 0.0;
            K[1, 2] = 0.0;
            K[1, 3] = 0.0;
            K[2, 0] = -K[0, 0];
            K[2, 1] = 0.0;
            K[2, 2] = K[0, 0];
            K[2, 3] = coef * k2;
            K[3, 0] = -coef * k2;
            K[3, 1] = 0.0;
            K[3, 2] = coef * k2;
            K[3, 3] = 3.0 * E * I / L;
            return K;
        }

        public static Matrix<double> Normal(double E, double A, double L)
        {
            var K = Matrix<double>.Build.Dense(2, 2);
            K[0, 0] = (E * A / L);
            K[0, 1] = -(E * A / L);
            K[1, 0] = -(E * A / L);
            K[1, 1] = (E * A / L);
            return K;
        }

        public static Matrix<double> TorsionFixed(double G, double It, double L)
        {
            var K = Matrix<double>.Build.Dense(2, 2);
            K[0, 0] = G * It / L;
            K[0, 1] = -(G * It / L);
            K[1, 0] = -(G * It / L);
            K[1, 1] = G * It / L;
            return K;
        }

        public static Matrix<double> TorsionHingedFixed(double G, double It, double L)
        {
            var K = Matrix<double>.Build.Dense(2, 2);
            K[0, 0] = G * It / L;
            K[0, 1] = 0;
            K[1, 0] = 0;
            K[1, 1] = 0;
            return K;
        }
        public static Matrix<double> TorsionFixedHinged(double G, double It, double L)
        {
            var K = Matrix<double>.Build.Dense(2, 2);
            K[0, 0] = 0;
            K[0, 1] = 0;
            K[1, 0] = 0;
            K[1, 1] = G * It / L;
            return K;
        }

        public static Matrix<double> Stiffness(IGeometry section, IMaterial material, IsHinged hingeLeft, IsHinged hingeRight, double L)
        {
            Matrix<double> normal = Normal(material.E, section.A, L);
            Matrix<double> torsion = Matrix<double>.Build.Dense(2, 2);
            Matrix<double> flexionY = Matrix<double>.Build.Dense(4, 4);
            Matrix<double> flexionZ = Matrix<double>.Build.Dense(4, 4);
            if (hingeLeft.X && hingeRight.X)
            {
                throw new NotImplementedException("Torsion hinged-hinged not implemented yet.");

            }
            else if (!hingeLeft.X && hingeRight.X)
            {
                torsion = TorsionFixedHinged(material.G, section.I_t, L);
            }
            else if (hingeLeft.X && !hingeRight.X)
            {
                torsion = TorsionHingedFixed(material.G, section.I_t, L);
            }
            else if (!hingeLeft.X && !hingeRight.X)
            {
                torsion = TorsionFixed(material.G, section.I_t, L);
            }
            if (hingeLeft.Y && hingeRight.Y)
            {
                throw new NotImplementedException("Flexion hinged-hinged not implemented yet.");
            }
            else if (!hingeLeft.Y && hingeRight.Y)
            {
                flexionY = FlexionFixedHinged(material.E, material.G, section.I_z, L, true);
            }
            else if (hingeLeft.Y && !hingeRight.Y)
            {
                flexionY = FlexionHingedFixed(material.E, material.G, section.I_z, L, true);
            }
            else if (!hingeLeft.Y && !hingeRight.Y)
            {
                flexionY = FlexionFixed(material.E, material.G, section.I_z, section.A_y, L, true);
            }
            if (hingeLeft.Z && hingeRight.Z)
            {
                throw new NotImplementedException("Flexion hinged-hinged not implemented yet.");
            }
            else if (!hingeLeft.Z && hingeRight.Z)
            {
                flexionZ = FlexionFixedHinged(material.E, material.G, section.I_y, L, false);
            }
            else if (hingeLeft.Z && !hingeRight.Z)
            {
                flexionZ = FlexionHingedFixed(material.E, material.G, section.I_y, L, false);
            }
            else if (!hingeLeft.Z && !hingeRight.Z)
            {
                flexionZ = FlexionFixed(material.E, material.G, section.I_y, section.A_z, L, false);
            }
            // Build assembly matrices and validate dimensions before assembling
            var asmNormal = AssemblyMatrixClass.GetAssemblyMatrix(12, new System.Collections.Generic.List<int> { 0, 6 });
            if (asmNormal.RowCount != normal.ColumnCount)
                throw new ArgumentException($"Assembly matrix columns ({asmNormal.ColumnCount}) do not match normal rows ({normal.RowCount}). Connectivity: [0,6]");
            normal = AssemblyMatrixClass.AssembleMatrix(asmNormal, normal);

            var asmFlexY = AssemblyMatrixClass.GetAssemblyMatrix(12, new System.Collections.Generic.List<int> { 1, 5, 7, 11 });
            if (asmFlexY.RowCount != flexionY.ColumnCount)
                throw new ArgumentException($"Assembly matrix columns ({asmFlexY.ColumnCount}) do not match flexionY rows ({flexionY.RowCount}). Connectivity: [1,5,7,11]");
            flexionY = AssemblyMatrixClass.AssembleMatrix(asmFlexY, flexionY);

            var asmFlexZ = AssemblyMatrixClass.GetAssemblyMatrix(12, new System.Collections.Generic.List<int> { 2, 4, 8, 10 });
            if (asmFlexZ.RowCount != flexionZ.ColumnCount)
                throw new ArgumentException($"Assembly matrix columns ({asmFlexZ.ColumnCount}) do not match flexionZ rows ({flexionZ.RowCount}). Connectivity: [2,4,8,10]");
            flexionZ = AssemblyMatrixClass.AssembleMatrix(asmFlexZ, flexionZ);

            var asmTors = AssemblyMatrixClass.GetAssemblyMatrix(12, new System.Collections.Generic.List<int> { 3, 9 });
            if (asmTors.RowCount != torsion.ColumnCount)
                throw new ArgumentException($"Assembly matrix columns ({asmTors.ColumnCount}) do not match torsion rows ({torsion.RowCount}). Connectivity: [3,9]");
            torsion = AssemblyMatrixClass.AssembleMatrix(asmTors, torsion);
            return normal + flexionY + flexionZ + torsion;
        }

        // Assembly helpers (thin wrappers delegating to AssemblyMatrixClass)
        public static Vector<double> AssembleVector(Matrix<double> assemblyMatrix, Vector<double> vectorToAssemble)
        {
            return AssemblyMatrixClass.AssembleVector(assemblyMatrix, vectorToAssemble);
        }

        public static Matrix<double> GetAssemblyMatrix(int size, System.Collections.Generic.List<int> connectivity)
        {
            return AssemblyMatrixClass.GetAssemblyMatrix(size, connectivity);
        }

        public static Matrix<double> AssembleMatrix(Matrix<double> assemblyMatrix, Matrix<double> matrixToAssemble)
        {
            return AssemblyMatrixClass.AssembleMatrix(assemblyMatrix, matrixToAssemble);
        }

        /// <summary>
        /// Compute stiffness matrix for spring connections (diagonal springs between nodes).
        /// </summary>
        public static Matrix<double> SpringStiffness(double stiffness_ux, double stiffness_uy, double stiffness_uz,
                                                     double stiffness_theta_x, double stiffness_theta_y, double stiffness_theta_z)
        {
            var K = Matrix<double>.Build.Dense(12, 12);
            // Translating the C++ assignments
            K[0, 0] = (double)stiffness_ux; K[0, 6] = (double)-stiffness_ux;
            K[1, 1] = (double)stiffness_uy; K[1, 7] = (double)stiffness_uy;
            K[2, 2] = (double)stiffness_uz; K[2, 8] = (double)stiffness_uz;
            K[3, 3] = (double)stiffness_theta_x; K[3, 9] = (double)stiffness_theta_x;
            K[4, 4] = (double)stiffness_theta_y; K[4, 10] = (double)stiffness_theta_y;
            K[5, 5] = (double)stiffness_theta_z; K[5, 11] = (double)stiffness_theta_z;
            K[6, 0] = (double)-stiffness_ux; K[6, 6] = (double)stiffness_ux;
            K[7, 1] = (double)stiffness_uy; K[7, 7] = (double)stiffness_uy;
            K[8, 2] = (double)stiffness_uz; K[8, 8] = (double)stiffness_uz;
            K[9, 3] = (double)stiffness_theta_x; K[9, 9] = (double)stiffness_theta_x;
            K[10, 4] = (double)stiffness_theta_y; K[10, 10] = (double)stiffness_theta_y;
            K[11, 5] = (double)stiffness_theta_z; K[11, 11] = (double)stiffness_theta_z;

            return K;
        }
    }
}
