using System;
using System.Collections.Generic;
using Ofel.Core.SectionParameter;

namespace Ofel.Core
{
    public class MemberDirector
    {
        /// <summary>
        /// Backwards-compatible overload without explicit assembly flag (defaults to true).
        /// </summary>
        public void SetPointsDataLinear(Member member, Point start, Point end, IGeometry geometry, IMaterial material)
        {
            PointMemberData pmd = new PointMemberData(0f, start, geometry);
            PointMemberData pmd2 = new PointMemberData(1f, end, geometry);
            member.PointsData.Clear();
            member.PointsData.Add(pmd);
            member.PointsData.Add(pmd2);
            member.Material = material;
        }

        /// <summary>
        /// Adds a Force to an existing Member and returns the Member for chaining.
        /// </summary>
        public void AddForce(Member member, Force force)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (force == null) throw new ArgumentNullException(nameof(force));
            member.AddForce(force);
        }

        public void AddHingedSupport(Member member, double epsilon, bool isXRotationFixed = false, bool isYRotationFixed = false, bool isZRotationFixed = false)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar(epsilon, new DegreesOfFreedom(false, false, false, !isXRotationFixed, !isYRotationFixed, !isZRotationFixed));
            member.AddCharacteristic(supportChar);
        }

        public void AddFixedSupport(Member member, double epsilon)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar(epsilon, new DegreesOfFreedom(false, false, false, false, false, false));
            member.AddCharacteristic(supportChar);
        }

        public void AddHingedSupport(Member member, double epsilon, DegreesOfFreedom dof)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar(epsilon, dof);
            member.AddCharacteristic(supportChar);
        }

        /// <summary>
        /// Sets the Roll property on the member (useful to apply a local rotation) and returns the Member.
        /// </summary>
        public void SetRoll(Member member, double roll)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            member.Roll = roll;
        }

        /// <summary>
        /// Adds a single characteristic to the member and returns it.
        /// </summary>
        public void AddCharacteristic(Member member, Characteristic characteristic)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (characteristic == null) throw new ArgumentNullException(nameof(characteristic));
            member.AddCharacteristic(characteristic);
        }

        /// <summary>
        /// Adds multiple characteristics to the member and returns it.
        /// Null characteristics in the enumerable are ignored.
        /// </summary>
        public void SetCharacteristics(Member member, IEnumerable<Characteristic> characteristics)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (characteristics == null) throw new ArgumentNullException(nameof(characteristics));
            foreach (var c in characteristics)
            {
                if (c != null) member.AddCharacteristic(c);
            }
        }

        public void SetForces(Member member, IEnumerable<Force> forces)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (forces == null) throw new ArgumentNullException(nameof(forces));
            foreach (var f in forces)
            {
                if (f != null) member.AddForce(f);
            }
        }
    }
}
