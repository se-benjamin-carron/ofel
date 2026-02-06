using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelFlexionInput
    {
        public Axis Axis { get; init; }
        public required SteelVerificationContext Context { get; init; }
        public required double ReductionCoef { get; init; }
    }


    public abstract class SteelFlexion : IVerifiable
    {
        public abstract Axis Axis { get; }
        public abstract int SectionClass { get; }

        public abstract double MEd { get; }
        public abstract double W { get; }
        public abstract double Fy { get; }
        public abstract double GammaM0 { get; }
        public abstract double ReductionCoef { get; }

        public double MRd => W * Fy / GammaM0 * ReductionCoef;

        public double Ratio => Math.Abs(MEd / MRd);

        public abstract Verification ToVerification();
    }


    public static class SteelFlexionFactory
    {
        public static SteelFlexion Create(SteelFlexionInput Input)
        {
            return (Input.Context.SectionClass.DesignClass, Input.Axis) switch
            {
                (1 or 2, Axis.Y) => new SteelFlexionResult_Class12_Y(Input),
                (1 or 2, Axis.Z) => new SteelFlexionResult_Class12_Z(Input),

                (3, Axis.Y) => new SteelFlexionResult_Class3_Y(Input),
                (3, Axis.Z) => new SteelFlexionResult_Class3_Z(Input),

                (4, Axis.Y) => new SteelFlexionResult_Class4_Y(Input),
                (4, Axis.Z) => new SteelFlexionResult_Class4_Z(Input),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
