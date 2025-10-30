using System.Numerics;

namespace ofel.Core
{
    /// <summary>
    /// Minimal rotation helper: provides conversion from global vector to local member coordinates.
    /// In a real implementation this would hold orientation matrices or quaternions.
    /// </summary>
    public sealed class RotationData
    {
        // For now we store a 3x3 rotation matrix in row-major order
        private readonly Matrix4x4 _matrix;

        public RotationData(Matrix4x4 matrix)
        {
            _matrix = matrix;
        }

        public Vector3 ToLocal(Vector3 global)
        {
            // multiply by the inverse rotation (transpose if pure rotation)
            // assume _matrix is orthonormal rotation -> use transpose
            var m = _matrix;
            return new Vector3(
                global.X * m.M11 + global.Y * m.M21 + global.Z * m.M31,
                global.X * m.M12 + global.Y * m.M22 + global.Z * m.M32,
                global.X * m.M13 + global.Y * m.M23 + global.Z * m.M33
            );
        }
    }

    /// <summary>
    /// Minimal epsilon data carrier. Extend as needed to represent geometric epsilon/offsets.
    /// </summary>
    public sealed class EpsilonData
    {
        public float Epsilon { get; }

        // Optional override force value: some calculations may provide the load value directly
        public ForceValue? ForceOverride { get; }

        public EpsilonData(float epsilon, ForceValue? forceOverride = null)
        {
            Epsilon = epsilon;
            ForceOverride = forceOverride;
        }
    }
}
