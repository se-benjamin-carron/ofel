using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelNormalInput()
    {
        public required SteelVerificationContext Context { get; init; }
        public required double ReductionCoef { get; init; }
    }

    public abstract class SteelNormal : IVerifiable
    {
        public abstract int SectionClass { get; }

        public abstract double N_Ed { get; }
        public abstract double A { get; }
        public abstract double Fy { get; }

        public abstract double GammaM0 { get; }
        public double N_Rd => A * Fy / GammaM0;

        public double Ratio => Math.Abs(N_Ed / N_Rd);

        public abstract Verification ToVerification();
    }

    public static class SteelNormalFactory
    {
        public static SteelNormal Create(SteelNormalInput input)
        {
            return (input.Context.SectionClass.DesignClass, input.Context.Section.ProfileType) switch
            {
                (1 or 2 or 3, _) => new SteelNormal_Stable(input),

                (4, "HEA" or "IPE" or "PRS") => new SteelNormal_InstableIPE(input),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
