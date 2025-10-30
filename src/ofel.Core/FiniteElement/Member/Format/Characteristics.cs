using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace ofel.Core
{
    public interface ICharacteristic
    {
        string Kind { get; }

        /// <summary>
        /// Return the epsilon positions (0..1) that this characteristic concerns.
        /// Cannot be empty (no epsilon), one value, or multiple (e.g. haunch start/end).
        /// </summary>
        IEnumerable<double> GetEpsilons();
        MainEpsilon[] ToMainEpsilon();
    }

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
  
    public class SupportChar : ICharacteristic
    {
        public string Kind => "support";
        public double Epsilon { get; }
        public DegreesOfFreedom DegreesOfFreedom { get; }

        public SupportChar(double epsilon, DegreesOfFreedom dof)
        {
            Epsilon = epsilon;
            DegreesOfFreedom = dof;
        }

        public IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.SupportChar) };
        }
    }

    public class HingeChar : ICharacteristic
    {
        public string Kind => "hinge";

        // Indique sur quels axes l'élément est articulé
        public IsHinged HingedAxes { get; }

        // epsilon associé à l'articulation
        public double Epsilon { get; }

        // sens de l'articulation par axe
        public Direction HingedDirection { get; }

        public HingeChar(IsHinged hingedAxes, double epsilon, Direction hingedDirection)
        {
            HingedAxes = hingedAxes;
            Epsilon = epsilon;
            HingedDirection = hingedDirection;
        }

        public IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] {
                new MainEpsilon(
                    Epsilon,
                    HingedDirection == Direction.Natural
                        ? KindMainEpsilon.NaturalHingeChar
                        : KindMainEpsilon.UnNaturalHingeChar
                ) };
        }
    }

    public class HaunchChar : ICharacteristic
    {
        public string Kind => "haunch";
        // epsilon au début et à la fin de la haunch
        public double EpsilonStart { get; }
        public double EpsilonEnd { get; }
        // ratio caractéristique de la haunch
        public double HaunchRatio { get; }

        public HaunchChar(double epsilonStart, double epsilonEnd, double haunchRatio)
        {
            EpsilonStart = epsilonStart;
            EpsilonEnd = epsilonEnd;
            HaunchRatio = haunchRatio;
        }

        public IEnumerable<double> GetEpsilons()
        {
            yield return EpsilonStart;
            yield return EpsilonEnd;
        }
        public MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] {
                new MainEpsilon(EpsilonStart, KindMainEpsilon.HaunchChar),
                new MainEpsilon(EpsilonEnd, KindMainEpsilon.HaunchChar)
            };
        }
    }

    public class AssemblyChar : ICharacteristic
    {
        public string Kind => "assembly";
        // Position epsilon associée à l'assembly
        public double Epsilon { get; }

        public AssemblyChar(double epsilon)
        {
            Epsilon = epsilon;
        }

        public IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.AssemblyChar) };
        }
    }

    public class SpringChar : ICharacteristic
    {
        public string Kind => "spring";
        public Spring Spring { get; }
        public double Epsilon { get; }

        public SpringChar(Spring spring, double epsilon)
        {
            Spring = spring;
            Epsilon = epsilon;
        }

        public IEnumerable<double> GetEpsilons()
        {
            yield return Epsilon;
        }
        public MainEpsilon[] ToMainEpsilon()
        {
            return new MainEpsilon[] { new MainEpsilon(Epsilon, KindMainEpsilon.SpringChar) };
        }
    }
}
