using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace Ofel.Core
{
    /// <summary>
    /// Structure explicite pour représenter les degrés de liberté libérés (released).
    /// true = released (mouvement autorisé), false = restrained (encastré)
    /// Ordre: translations X,Y,Z puis rotations X,Y,Z
    /// </summary>
    public struct DegreesOfFreedom
    {
        public bool IsTranslationXReleased { get; set; }
        public bool IsTranslationYReleased { get; set; }
        public bool IsTranslationZReleased { get; set; }
        public bool IsRotationXReleased { get; set; }
        public bool IsRotationYReleased { get; set; }
        public bool IsRotationZReleased { get; set; }
        public DegreesOfFreedom(bool tx, bool ty, bool tz, bool rx, bool ry, bool rz)
        {
            IsTranslationXReleased = tx;
            IsTranslationYReleased = ty;
            IsTranslationZReleased = tz;
            IsRotationXReleased = rx;
            IsRotationYReleased = ry;
            IsRotationZReleased = rz;
        }
    }

    /// <summary>
    /// Indique si l'axe est articulé (hinged) sur X/Y/Z
    /// </summary>
    public struct Spring
    {
        public double U_X { get; }
        public double U_Y { get; }
        public double U_Z { get; }
        public double T_X { get; }
        public double T_Y { get; }
        public double T_Z { get; }

        public Spring(double u_x, double u_y, double u_z, double t_x, double t_y, double t_z)
        {
            U_X = u_x; U_Y = u_y; U_Z = u_z;
            T_X = t_x; T_Y = t_y; T_Z = t_z;
        }
    }

    /// <summary>
    /// Indique si l'axe est articulé (hinged) sur X/Y/Z
    /// </summary>
    public struct IsHinged
    {
        public bool X { get; }
        public bool Y { get; }
        public bool Z { get; }

        public IsHinged(bool x, bool y, bool z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// Direction possible d'articulation
    /// </summary>
    public enum Direction
    {
        None = 0,
        Natural = 1,
        Inverse = -1
    }

    public abstract class Characteristic
    {
        public string Kind { get; protected set; }
        public Characteristic(string kindChar)
        {
            Kind = kindChar;
        }

        public abstract Characteristic Clone();
        public abstract IEnumerable<double> GetEpsilons();
        public abstract MainEpsilon[] ToMainEpsilon();

    }

    public abstract class Characteristic<TSelf> : Characteristic
    where TSelf : Characteristic<TSelf>
    {
        public Characteristic(string name) : base(name) { }

        protected abstract TSelf CloneTyped();

        public sealed override Characteristic Clone()
            => CloneTyped();
    }

    public class SupportChar : Characteristic<SupportChar>
    {
        public double Epsilon { get; }
        public DegreesOfFreedom DegreesOfFreedom { get; }

        public SupportChar(double epsilon, DegreesOfFreedom dof) : base("support")
        {
            Epsilon = epsilon;
            DegreesOfFreedom = dof;
        }

        public override IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public override MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.SupportChar) };
        }
        protected override SupportChar CloneTyped()
        {
            return new SupportChar(Epsilon, DegreesOfFreedom);
        }
    }

    public class HingeChar : Characteristic<HingeChar>
    {
        // Indique sur quels axes l'élément est articulé
        public IsHinged HingedAxes { get; }

        // epsilon associé à l'articulation
        public double Epsilon { get; }

        // sens de l'articulation par axe
        public Direction HingedDirection { get; }

        public HingeChar(IsHinged hingedAxes, double epsilon, Direction hingedDirection) : base("hinge")
        {
            HingedAxes = hingedAxes;
            Epsilon = epsilon;
            HingedDirection = hingedDirection;
        }

        public override IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public override MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] {
                new MainEpsilon(
                    Epsilon,
                    HingedDirection == Direction.Natural
                        ? KindMainEpsilon.NaturalHingeChar
                        : KindMainEpsilon.UnNaturalHingeChar
                ) };
        }
        protected override HingeChar CloneTyped()
        {
            return new HingeChar(HingedAxes, Epsilon, HingedDirection);
        }
    }

    public class HaunchChar : Characteristic<HaunchChar>
    {
        // epsilon au début et à la fin de la haunch
        public double EpsilonStart { get; }
        public double EpsilonEnd { get; }
        // ratio caractéristique de la haunch
        public double HaunchRatio { get; }

        public HaunchChar(double epsilonStart, double epsilonEnd, double haunchRatio) : base("haunch")
        {
            EpsilonStart = epsilonStart;
            EpsilonEnd = epsilonEnd;
            HaunchRatio = haunchRatio;
        }

        public override IEnumerable<double> GetEpsilons()
        {
            yield return EpsilonStart;
            yield return EpsilonEnd;
        }
        public override MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] {
                new MainEpsilon(EpsilonStart, KindMainEpsilon.HaunchChar),
                new MainEpsilon(EpsilonEnd, KindMainEpsilon.HaunchChar)
            };
        }

        protected override HaunchChar CloneTyped()
        {
            return new HaunchChar(EpsilonStart, EpsilonEnd, HaunchRatio);
        }
    }

    public class AssemblyChar : Characteristic<AssemblyChar>
    {
        // Position epsilon associée à l'assembly
        public double Epsilon { get; }

        public AssemblyChar(double epsilon) : base("assembly")
        {
            Epsilon = epsilon;
        }

        public override IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public override MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.AssemblyChar) };
        }
        protected override AssemblyChar CloneTyped()
        {
            return new AssemblyChar(Epsilon);
        }
    }

    public class SpringChar : Characteristic<SpringChar>
    {
        public Spring Spring { get; }
        public double Epsilon { get; }

        public SpringChar(Spring spring, double epsilon) : base("spring")
        {
            Spring = spring;
            Epsilon = epsilon;
        }

        public override IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public override MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.SpringChar) };
        }
        protected override SpringChar CloneTyped()
        {
            return new SpringChar(Spring, Epsilon);
        }
    }
}
