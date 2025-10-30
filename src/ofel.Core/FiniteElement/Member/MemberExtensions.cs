using System;
using System.Collections.Generic;

namespace ofel.Core
{
    /// <summary>
    /// Fluent extension helpers for <see cref="Member"/> to enable chaining without
    /// changing the existing instance API.
    /// Example: var m = new Member(...).WithForce(f).WithRoll(0.1).WithFixedSupport(0);
    /// </summary>
    public static class MemberExtensions
    {
        public static Member WithForce(this Member member, Force force)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (force == null) throw new ArgumentNullException(nameof(force));
            member.AddForce(force);
            return member;
        }

        public static Member WithHingedSupport(this Member member, double epsilon, bool isXRotationFixed = true, bool isYRotationFixed = true, bool isZRotationFixed = true)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar((float)epsilon, new DegreesOfFreedom(false, false, false, !isXRotationFixed, !isYRotationFixed, !isZRotationFixed));
            member.AddCharacteristic(supportChar);
            return member;
        }

        public static Member WithFixedSupport(this Member member, double epsilon)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var supportChar = new SupportChar((float)epsilon, new DegreesOfFreedom(false, false, false, false, false, false));
            member.AddCharacteristic(supportChar);
            return member;
        }

        public static Member WithRoll(this Member member, double roll)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            member.Roll = roll;
            return member;
        }

        public static Member WithCharacteristic(this Member member, ICharacteristic characteristic)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (characteristic == null) throw new ArgumentNullException(nameof(characteristic));
            member.AddCharacteristic(characteristic);
            return member;
        }

        public static Member WithCharacteristics(this Member member, IEnumerable<ICharacteristic> characteristics)
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
}
