using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace ofel.Core
{
    /// <summary>
    /// Représente un élément (Member) défini par un dictionnaire de points, une géométrie et un matériau.
    /// Les propriétés sont modifiables après construction (mutabilité autorisée).
    /// </summary>
    public sealed class Member
    {
        // List of PointData linking epsilon (float) to a geometry and
        // all the characteristics needed in order to perform the FE analysis
        // this enables to have different geometry along the member and curves shape
        public List<PointMemberData> PointsData { get; } = new List<PointMemberData>();

        // liste de caractéristiques attachées au member
        public List<ICharacteristic> Characteristics { get; } = new List<ICharacteristic>();

        // liste de forces appliquées au member
        public List<Force> Forces { get; } = new List<Force>();

        // Material that can be modified after construction
        public Material Material { get; set; }

        public Member(List<PointMemberData> points_data, Material material)
        {

            PointsData = new List<PointMemberData>(points_data);
            Material = material;
            Length = GetMemberLength();
        }

        public double Length { get; set; } = 0.0;

        public double Roll { get; set; } = 0.0;

        // Helper methods to manage points
        public void AddPointData(PointMemberData p)
        {
            if (p == null) throw new System.ArgumentNullException(nameof(p));
            PointsData.Add(p);
            SortPointData();
            Length = GetMemberLength();
        }

        public void RemovePointData(PointMemberData p)
        {
            if (p == null) throw new System.ArgumentNullException(nameof(p));

            PointsData.Remove(p);
            Length = GetMemberLength();

        }

        // Trie la liste _pointsData selon Epsilon croissant
        public void SortPointData()
        {
            var sorted = PointsData.OrderBy(pd => pd.Epsilon).ToList();
            PointsData.Clear();
            foreach (var pd in sorted)
                PointsData.Add(pd);
        }

        // characteristic helpers
        public void AddCharacteristic(ICharacteristic c)
        {
            if (c == null) throw new System.ArgumentNullException(nameof(c));
            Characteristics.Add(c);
        }

        public bool RemoveCharacteristic(ICharacteristic c)
        {
            return Characteristics.Remove(c);
        }

        public IEnumerable<ICharacteristic> GetCharacteristicsByKind(string kind)
        {
            foreach (var c in Characteristics)
                if (string.Equals(c.Kind, kind, System.StringComparison.OrdinalIgnoreCase)) yield return c;
        }
        public Spring getSpringDataFromEpsilon(double epsilon)
        {
            foreach (ICharacteristic c in Characteristics)
            {
                if (c is SpringChar sc && Math.Abs(sc.Epsilon - epsilon) < 1e-6)
                {
                    return sc.Spring;
                }
            }
            return new Spring(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
        }

        public DegreesOfFreedom getSupportDataFromEpsilon(double epsilon)
        {
            foreach (ICharacteristic c in Characteristics)
            {
                if (c is SupportChar sc && Math.Abs(sc.Epsilon - epsilon) < 1e-6)
                {
                    return sc.DegreesOfFreedom;
                }
            }
            return new DegreesOfFreedom(true, true, true, true, true, true);
        }

        public IsHinged getHingedConditionFromEpsilon(double epsilon)
        {
            foreach (ICharacteristic c in Characteristics)
            {
                if (c is HingeChar hc && Math.Abs(hc.Epsilon - epsilon) < 1e-6)
                {
                    return hc.HingedAxes;
                }
            }
            return new IsHinged(false, false, false);
        }
        public bool IsAssembly(double epsilon)
        {
            foreach (ICharacteristic c in Characteristics)
            {
                if (c is AssemblyChar ac && Math.Abs(ac.Epsilon - epsilon) < 5e-7)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSupport(double epsilon)
        {
            foreach (ICharacteristic c in Characteristics)
            {
                if (c is SupportChar sc && Math.Abs(sc.Epsilon - epsilon) < 5e-7)
                {
                    return true;
                }
            }
            return false;
        }

        // force helpers
        public void AddForce(Force f)
        {
            if (f == null) throw new System.ArgumentNullException(nameof(f));
            Forces.Add(f);
        }

        public bool RemoveForce(Force f)
        {
            return Forces.Remove(f);
        }

        public IEnumerable<Force> GetForcesByKind(ForceKind kind)
        {
            foreach (var f in Forces)
                if (f.Kind == kind) yield return f;
        }

        /// <summary>
        /// Compute total member length by summing distances between consecutive points.
        /// </summary>
        public double GetMemberLength()
        {
            var pts = new List<PointMemberData>(PointsData);
            if (pts.Count < 2) return 0.0;
            double length = 0.0;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = pts[i].Point;
                var p1 = pts[i + 1].Point;
                length += p0.DistanceTo(p1);
            }
            return length;
        }

        /// <summary>
        /// Returns the geometry at a given normalized position (epsilon in [0,1]) along the member.
        /// If geometries differ between adjacent points, selects nearest neighbor based on interpolation weight.
        /// </summary>
        public IGeometry GetInterpolatedGeometry(double epsilon)
        {
            if (epsilon < 0f || epsilon > 1f)
                throw new System.ArgumentOutOfRangeException(nameof(epsilon), "epsilon must be between 0 and 1");
            var pts = PointsData.OrderBy(pd => pd.Epsilon).ToList();
            if (pts.Count == 0)  throw new System.InvalidOperationException("No point data available.");
            if (epsilon <= pts.First().Epsilon) return pts.First().Geometry;
            if (epsilon >= pts.Last().Epsilon) return pts.Last().Geometry;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = pts[i];
                var p1 = pts[i + 1];
                if (epsilon >= p0.Epsilon && epsilon <= p1.Epsilon)
                {
                    double t = (epsilon - p0.Epsilon) / (p1.Epsilon - p0.Epsilon);
                    // interpolate geometry attributes
                    return p0.Geometry.Interpolate(p1.Geometry, t);
                }
            }
            // fallback to last geometry
            return pts.Last().Geometry;
        }

        /// <summary>
        /// Returns the interpolated point coordinates at a given epsilon along the member.
        /// </summary>
        public Point GetInterpolatedPoint(double epsilon)
        {
            if (epsilon < 0f || epsilon > 1f)
                throw new System.ArgumentOutOfRangeException(nameof(epsilon), "epsilon must be between 0 and 1");
            var pts = PointsData.OrderBy(pd => pd.Epsilon).ToList();
            if (pts.Count == 0) throw new InvalidOperationException("No point data available.");
            if (epsilon <= pts.First().Epsilon) return pts.First().Point;
            if (epsilon >= pts.Last().Epsilon) return pts.Last().Point;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = pts[i];
                var p1 = pts[i + 1];
                if (epsilon >= p0.Epsilon && epsilon <= p1.Epsilon)
                {
                    double t = (epsilon - p0.Epsilon) / (p1.Epsilon - p0.Epsilon);
                    double x = p0.Point.X + t * (p1.Point.X - p0.Point.X);
                    double y = p0.Point.Y + t * (p1.Point.Y - p0.Point.Y);
                    double z = p0.Point.Z + t * (p1.Point.Z - p0.Point.Z);
                    return new Point(x, y, z);
                }
            }
            throw new InvalidOperationException("Could not interpolate point for given epsilon.");
        }
    }
}
