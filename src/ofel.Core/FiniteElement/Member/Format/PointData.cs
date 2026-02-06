using System;
using Ofel.Core.SectionParameter;

namespace Ofel.Core
{
    /// <summary>
    /// Associates a normalized position (epsilon) with a Point and its Geometry.
    /// </summary>
    public sealed class PointMemberData
    {
        /// <summary>
        /// Normalized position along the member, between 0 and 1.
        /// </summary>
        public double Epsilon { get; set; }

        /// <summary>
        /// The point coordinates.
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// The geometry at this point.
        /// </summary>
        public IGeometry Geometry { get; set; }

        public PointMemberData(double epsilon, Point point, IGeometry geometry)
        {
            if (epsilon < 0.0 || epsilon > 1.0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "epsilon must be between 0 and 1");
            Epsilon = epsilon;
            Point = point ?? throw new ArgumentNullException(nameof(point));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public PointMemberData Clone()
        {
            return new PointMemberData(Epsilon, Point.Clone(), Geometry.Clone());
        }
    }
    public sealed class PointStructureData
    {
        public Point Point { get; set; }
        public bool IsSupport { get; set; } = false;
        public bool IsAssembly { get; set; } = false;
        public DegreesOfFreedom SupportConditions { get; set; }

        public int id { get; set; }

        public PointStructureData(Point point, bool is_assembly, bool is_support, DegreesOfFreedom supportConditions, ref int nextId)
        {
            Point = point ?? throw new ArgumentNullException(nameof(point));
            IsAssembly = is_assembly;
            IsSupport = is_support;
            SupportConditions = supportConditions;

            id = nextId++;
        }
        public bool IsPossibleToSharePoint()
        {
            return IsAssembly || IsSupport;
        }

        public List<int> FixedDirectionsIndices()
        {
            List<int> fixed_indices = new List<int>();
            if (!SupportConditions.IsTranslationXReleased) fixed_indices.Add(id * 6 + 0);
            if (!SupportConditions.IsTranslationYReleased) fixed_indices.Add(id * 6 + 1);
            if (!SupportConditions.IsTranslationZReleased) fixed_indices.Add(id * 6 + 2);
            if (!SupportConditions.IsRotationXReleased) fixed_indices.Add(id * 6 + 3);
            if (!SupportConditions.IsRotationYReleased) fixed_indices.Add(id * 6 + 4);
            if (!SupportConditions.IsRotationZReleased) fixed_indices.Add(id * 6 + 5);
            return fixed_indices;
        }
        public List<int> FreeDirectionsIndices()
        {
            List<int> free_indices = new List<int>();
            if (SupportConditions.IsTranslationXReleased) free_indices.Add(id * 6 + 0);
            if (SupportConditions.IsTranslationYReleased) free_indices.Add(id * 6 + 1);
            if (SupportConditions.IsTranslationZReleased) free_indices.Add(id * 6 + 2);
            if (SupportConditions.IsRotationXReleased) free_indices.Add(id * 6 + 3);
            if (SupportConditions.IsRotationYReleased) free_indices.Add(id * 6 + 4);
            if (SupportConditions.IsRotationZReleased) free_indices.Add(id * 6 + 5);
            return free_indices;
        }
    }
}
