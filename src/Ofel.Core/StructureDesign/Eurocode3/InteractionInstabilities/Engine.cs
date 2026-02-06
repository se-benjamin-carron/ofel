using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using Ofel.Core.StructureDesign.Eurocode3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Ofel.Core.StructureDesign.Eurocode3.SteelBucklingFactory;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public enum InteractionInstabilitiesType
    {
        AnnexA,
        AnnexB,
        NotConcerned,
    }

    public sealed class ShapeCoefficient
    {
        public double Cmy0 { get; init; }
        public double Cmz0 { get; init; }

        private ShapeCoefficient(double cmy0, double cmz0)
        {
            Cmy0 = cmy0;
            Cmz0 = cmz0;
        }
        public static ShapeCoefficient CreateDefault()
        {
            return new ShapeCoefficient(1, 1);
        }

    }
    public class SteelInteractionInstabilitiesInput
    {

        public required SteelVerificationContext Context { get; init; }
        public required InteractionInstabilitiesType Type { get; init; }
        public required SteelNormal Normal { get; init; }
        public required SteelFlexion FlexionY { get; init; }
        public required SteelFlexion FlexionZ { get; init; }
        public required SteelBuckling BucklingY { get; init; }
        public required SteelBuckling BucklingZ { get; init; }
        public required SteelBucklingTorsionBase Torsion { get; init; }
        public required SteelLateralBuckling LateralBuckling { get; init; }
        public required FlexionCoefficient CoefficientC1C2 { get; init; }
        public required ShapeCoefficient Cmi0 { get; init; }
    }


    public static class SteelInteractionInstabilitiesFactory
    {
        public static SteelInteractionInstabilities Create(SteelInteractionInstabilitiesInput input)
        {
            return (input.Type, input.Context.SectionClass.DesignClass) switch
            {
                (InteractionInstabilitiesType.AnnexA, 1 or 2) => new SteelInteractionInstabilities_AnnexA_12(input),
                (InteractionInstabilitiesType.AnnexA, 3) => new SteelInteractionInstabilities_AnnexA_3(input),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public abstract class SteelInteractionInstabilities : IVerifiable
    {
        public abstract Verification ToVerification();
        public abstract double RatioY { get; }
        public abstract double RatioZ { get; }
        public double Ratio => Math.Max(RatioY, RatioZ);
    }
}
