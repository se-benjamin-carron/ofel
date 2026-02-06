using System;
using System.Formats.Asn1;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra;
using Ofel.Core.SectionParameter;

namespace Ofel.Core
{

    public enum KindMainEpsilon
    {
        Default,
        Start,
        End,
        Force,
        SpringChar,
        AssemblyChar,
        HaunchChar,
        SupportChar,
        NaturalHingeChar,
        UnNaturalHingeChar,
    }
    /// <summary>
    /// Associates a normalized position (epsilon) with a Point and its Geometry.
    /// </summary>
    public class MainEpsilon
    {
        /// <summary>
        /// Float value of the epsilon between 0 and 1
        /// </summary>
        public double Epsilon { get; set; }
        public IGeometry? Geometry { get; set; }
        public IMaterial? Material { get; set; }

        public KindMainEpsilon Kind { get; set; }

        public MainEpsilon(double epsilon, KindMainEpsilon kind)
        {
            if (epsilon < 0f || epsilon > 1f)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "epsilon must be between 0 and 1");
            Epsilon = epsilon;
            Kind = kind;
        }
        public void SetGeometry(IGeometry geometry)
        {
            Geometry = geometry;
        }
        public void SetMaterial(IMaterial material)
        {
            Material = material;
        }

        public List<float> GetNeighbouringEpsilons()
        {
            switch (Kind)
            {
                case KindMainEpsilon.Start:
                case KindMainEpsilon.End:
                case KindMainEpsilon.Default:
                    return new List<float>();
                case KindMainEpsilon.Force:
                case KindMainEpsilon.SpringChar:
                case KindMainEpsilon.AssemblyChar:
                case KindMainEpsilon.HaunchChar:
                case KindMainEpsilon.SupportChar:
                    return new List<float> { (float)(Epsilon - 1e-6f), (float)(Epsilon + 1e-6f) };
                case KindMainEpsilon.NaturalHingeChar:
                    return new List<float> { (float)(Epsilon + 1e-6f) };
                case KindMainEpsilon.UnNaturalHingeChar:
                    return new List<float> { (float)(Epsilon - 1e-6f) };
                default:
                    return new List<float> { (float)Epsilon };
            }
        }
    }
}
