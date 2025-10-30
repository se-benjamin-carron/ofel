using System;
using System.Formats.Asn1;
using MathNet.Numerics.LinearAlgebra;
using Ofel.MatrixCalc;

namespace ofel.Core
{
    /// <summary>
    /// Associates a normalized position (epsilon) with a Point and its Geometry.
    /// </summary>
    public class IntervalData
    {
        /// <summary>
        /// Normalized start position of the interval along the member, between 0 and 1.
        /// </summary>
        public double Epsilon1 { get; set; }

        /// <summary>
        /// Normalized end position of the interval along the member, between 0 and 1.
        /// </summary>
        public double Epsilon2 { get; set; }

        /// <summary>
        /// Haunch ratio of the interval.
        /// </summary>
        public double HaunchRatio { get; set; }

        /// <summary>
        /// The geometry at this interval.
        /// </summary>
        public IGeometry Geometry { get; set; }

        /// <summary>
        /// The material at this interval.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// The angles (in radians) of the interval.
        /// </summary>
        public List<double> Angles { get; set; } = new List<double>();

        /// <summary>
        /// The rotation matrix of the interval.
        /// </summary>
        public Matrix<double> RotationMatrix { get; set; }

        /// <summary>
        /// The Stiffness Matrix at this interval
        /// </summary>
        public Matrix<double> StiffnessMatrix { get; set; }

        /// <summary>
        /// The Global Stiffness Matrix at this interval
        /// </summary>
        public Matrix<double> GlobalStiffnessMatrix { get; set; }

        /// <summary>
        /// The MassMatrix at this Interval
        /// </summary>
        public Matrix<double> MassMatrix { get; set; }

        /// <summary>
        /// The Global Mass Matrix at this interval
        /// </summary>
        public Matrix<double> GlobalMassMatrix { get; set; }
        /// <summary>
        /// The StressMatrix at this Interval
        /// </summary>
        public Matrix<double> UniformStressMatrix { get; set; }
        /// <summary>
        /// The Global Stress Matrix at this Interval
        /// </summary>
        public Matrix<double> GlobalUniformStressMatrix { get; set; }

        public Dictionary<string, MathNet.Numerics.LinearAlgebra.Vector<double>> LocalDisplacements
        {
            get; set;
        } = new Dictionary<string, MathNet.Numerics.LinearAlgebra.Vector<double>>();

        public Dictionary<string, MathNet.Numerics.LinearAlgebra.Vector<double>> InternalEfforts
        {
            get; set;
        } = new Dictionary<string, MathNet.Numerics.LinearAlgebra.Vector<double>>();

        public Point Point1 { get; set; }
        public Point Point2 { get; set; }

        public IntervalData(double epsilon1, double epsilon2, Point point1, Point point2, IGeometry geometry, Material material, double roll,
        KindMainEpsilon startKind, KindMainEpsilon endKind, IsHinged hingedCondition, Spring springData)
        {
            if (epsilon1 < 0.0 || epsilon1 > 1.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon1), "epsilon1 must be between 0 and 1");
            if (epsilon2 < 0.0 || epsilon2 > 1.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon2), "epsilon2 must be between 0 and 1");
            if (epsilon2 <= epsilon1)
                throw new ArgumentException("epsilon2 must be greater than epsilon1");
            Epsilon1 = epsilon1;
            Epsilon2 = epsilon2;
            Point1 = point1;
            Point2 = point2;
            Geometry = geometry;
            Material = material;
            Angles = ComputeAngles(Point1, Point2);
            Angles[2] = roll;
            // Build a block-diagonal rotation matrix for the element (two nodes × 6 DOF per node = 12x12)
            var R6 = RotationMatrixClass.SphericalRotation6x6(Angles[0], Angles[1], Angles[2]);
            RotationMatrix = RotationMatrixClass.ExtendGlobalRotationMatrix(R6, 2);
            // You may need to initialize these matrices properly before using them
            // For now, set them to null or initialize as needed
            StiffnessMatrix = GetStiffnessMatrixFromMainEpsilonTypes(startKind, endKind, geometry, material, hingedCondition, springData);
            MassMatrix = MassMatrixClass.GetMassMatrix(material.Rho, geometry.A, Point1.DistanceTo(Point2), geometry.I_t);
            UniformStressMatrix = UniformStressMatrixClass.GetUniformStressMatrix(Point1.DistanceTo(Point2));

            GlobalStiffnessMatrix = RotationMatrixClass.SwitchMatrixFromLocalToGlobal(RotationMatrix, StiffnessMatrix);
            GlobalMassMatrix = RotationMatrixClass.SwitchMatrixFromLocalToGlobal(RotationMatrix, MassMatrix);
            GlobalUniformStressMatrix = RotationMatrixClass.SwitchMatrixFromLocalToGlobal(RotationMatrix, UniformStressMatrix);
        }

        private List<double> ComputeAngles(Point point1, Point point2)
        {
            double length = point1.DistanceTo(point2);

            double delta_x = point2.X - point1.X;
            double delta_y = point2.Y - point1.Y;
            double delta_z = point2.Z - point1.Z;

            double cos_theta = delta_z / length;
            double theta = Math.Acos(cos_theta);

            double x2_y2 = Math.Sqrt(Math.Pow(delta_x, 2) + Math.Pow(delta_y, 2));
            double phi;

            if (delta_y > 0)
            {
                phi = Math.Acos(delta_x / x2_y2);
            }
            else if (delta_y < 0)
            {
                phi = 2 * Math.PI - Math.Acos(delta_x / x2_y2);
            }
            else
            {
                phi = Math.PI / 2;
            }

            return new List<double> { theta, phi, 0.0 };
        }

        private Matrix<double> GetStiffnessMatrixFromMainEpsilonTypes(KindMainEpsilon startKind, KindMainEpsilon endKind, IGeometry geometry, Material material, IsHinged hingedCondition, Spring springData)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            if (material == null) throw new ArgumentNullException(nameof(material));
            IsHinged isHingedNeutral = new IsHinged();

            double Length = Point1.DistanceTo(Point2);
            if (startKind == KindMainEpsilon.SpringChar && endKind == KindMainEpsilon.SpringChar)
            {
                return MatrixCalc.SpringStiffness(springData.U_X, springData.U_Y, springData.U_Z, springData.T_X, springData.T_Y, springData.T_Z);
            }
            else if (startKind == KindMainEpsilon.UnNaturalHingeChar || endKind == KindMainEpsilon.NaturalHingeChar)
            {
                return MatrixCalc.Stiffness(geometry, material, isHingedNeutral, hingedCondition, Length);
            }
            else if (startKind == KindMainEpsilon.NaturalHingeChar || endKind == KindMainEpsilon.UnNaturalHingeChar)
            {
                return MatrixCalc.Stiffness(geometry, material, hingedCondition, isHingedNeutral, Length);
            }

            // Default: no special hinge/spring characteristics -> use neutral hinges (both fixed as per Stiffness implementation)
            return MatrixCalc.Stiffness(geometry, material, isHingedNeutral, isHingedNeutral, Length);
        }
        public void computeInternalEfforts(string loading_case)
        {
            if (!LocalDisplacements.ContainsKey(loading_case))
                throw new ArgumentException($"No local displacements found for loading case '{loading_case}'.");
            else
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> localDisplacements = LocalDisplacements[loading_case];
                MathNet.Numerics.LinearAlgebra.Vector<double> internalEfforts = StiffnessMatrix * localDisplacements;
                InternalEfforts[loading_case] = internalEfforts;
            }
        }
        // Provide a proper override of ToString() so callers can write iv.ToString() or just iv in interpolation.
        public override string ToString()
        {
            return $"IntervalData [eps1={Epsilon1:0.######}, eps2={Epsilon2:0.######}, Point1={Point1?.ToString() ?? "null"}, Point2={Point2?.ToString() ?? "null"}]";
        }
    }
}
