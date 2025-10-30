using System;
using System.Collections.Generic;

namespace ofel.Core
{
    /// <summary>
    /// Abstract base director for building Member instances. Derived directors implement
    /// specific creation patterns (line, curve, ...).
    /// </summary>
    public abstract class MemberDirector
    {
        /// <summary>
        /// Create a member between two points (line). By default not supported by generic director.
        /// </summary>
        public virtual Member CreateFromTwoPoints(Point start, Point end, IGeometry geometry, Material material, bool isAssemblyAtEnds)
        {
            throw new NotSupportedException("CreateFromTwoPoints is not supported by this director");
        }

        /// <summary>
        /// Backwards-compatible overload without explicit assembly flag (defaults to true).
        /// </summary>
        public virtual Member CreateFromTwoPoints(Point start, Point end, IGeometry geometry, Material material)
        {
            return CreateFromTwoPoints(start, end, geometry, material, true);
        }

        /// <summary>
        /// Create a member from an ordered list of points (curve). By default not supported.
        /// </summary>
        public virtual Member CreateFromPointsList(IList<Point> points, IGeometry geometry, Material material, bool isAssemblyAtEnds)
        {
            throw new NotSupportedException("CreateFromPointsList is not supported by this director");
        }

        /// <summary>
        /// Backwards-compatible overload without explicit assembly flag (defaults to true).
        /// </summary>
        public virtual Member CreateFromPointsList(IList<Point> points, IGeometry geometry, Material material)
        {
            return CreateFromPointsList(points, geometry, material, true);
        }

        /// <summary>
        /// Adds a Force to an existing Member and returns the Member for chaining.
        /// </summary>
        public Member AddForce(Member member, Force force)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (force == null) throw new ArgumentNullException(nameof(force));
            member.AddForce(force);
            return member;
        }

        public Member AddHingedSupport(Member member, double epsilon, bool isXRotationFixed=true, bool isYRotationFixed=true, bool isZRotationFixed=true)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar(epsilon, new DegreesOfFreedom(false, false, false, !isXRotationFixed, !isYRotationFixed, !isZRotationFixed));
            member.AddCharacteristic(supportChar);
            return member;
        }

        public Member AddFixedSupport(Member member, double epsilon)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar(epsilon, new DegreesOfFreedom(false, false, false, false, false, false));
            member.AddCharacteristic(supportChar);
            return member;
        }

        /// <summary>
        /// Sets the Roll property on the member (useful to apply a local rotation) and returns the Member.
        /// </summary>
        public Member SetRoll(Member member, double roll)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            member.Roll = roll;
            return member;
        }

        /// <summary>
        /// Adds a single characteristic to the member and returns it.
        /// </summary>
        public Member AddCharacteristic(Member member, ICharacteristic characteristic)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (characteristic == null) throw new ArgumentNullException(nameof(characteristic));
            member.AddCharacteristic(characteristic);
            return member;
        }

        /// <summary>
        /// Adds multiple characteristics to the member and returns it.
        /// Null characteristics in the enumerable are ignored.
        /// </summary>
        public Member AddCharacteristics(Member member, IEnumerable<ICharacteristic> characteristics)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (characteristics == null) throw new ArgumentNullException(nameof(characteristics));
            foreach (var c in characteristics)
            {
                if (c != null) member.AddCharacteristic(c);
            }
            return member;
        }
    }

    /// <summary>
    /// Director that creates straight (line) members between two points.
    /// </summary>
    public class LineMemberDirector : MemberDirector
    {
        public override Member CreateFromTwoPoints(Point start, Point end, IGeometry geometry, Material material, bool isAssemblyAtEnds)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (end == null) throw new ArgumentNullException(nameof(end));
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            if (material == null) throw new ArgumentNullException(nameof(material));

            var points = new List<PointMemberData>
            {
                new PointMemberData(0f, start, geometry),
                new PointMemberData(1f, end, geometry)
            };

            var member = new Member(points, material);

            if (isAssemblyAtEnds)
            {
                member.AddCharacteristic(new AssemblyChar(0.0));
                member.AddCharacteristic(new AssemblyChar(1.0));
            }
            return member;
        }
    }

    /// <summary>
    /// Director that creates members from a list of points (curved members / polyline).
    /// </summary>
    public class CurveMemberDirector : MemberDirector
    {
        // Keep the overridden signature identical to the base class.
        public override Member CreateFromPointsList(IList<Point> points, IGeometry geometry, Material material, bool isAssemblyAtEnds)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2) throw new ArgumentException("points list must contain at least two points", nameof(points));
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            if (material == null) throw new ArgumentNullException(nameof(material));

            var pointData = new List<PointMemberData>();
            for (int i = 0; i < points.Count; i++)
            {
                double eps = (double)i / (points.Count - 1);
                pointData.Add(new PointMemberData((float)eps, points[i], geometry));
            }
            var member = new Member(pointData, material);
            // by default add assembly chars at ends for curve members
            if (isAssemblyAtEnds)
            {
                member.AddCharacteristic(new AssemblyChar(0.0));
                member.AddCharacteristic(new AssemblyChar(1.0));
            }

            return member;
        }
    }
}
