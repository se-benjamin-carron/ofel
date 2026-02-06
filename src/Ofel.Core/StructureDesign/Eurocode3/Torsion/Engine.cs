using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelTorsionInput()
    {
        public required SteelVerificationContext Context { get; init; }
    }

    public abstract class SteelTorsion : IVerifiable
    {
        public abstract double I_t { get; }
        public abstract double Mx { get; }
        public abstract double Fy { get; }
        public abstract double GammaM0 { get; }
        public abstract double Ratio { get; }
        public abstract Verification ToVerification();
    }

    public static class SteelTorsionFactory
    {
        public static SteelTorsion Create(SteelTorsionInput input)
        {
            return input.Context.Section.ProfileType switch
            {
                "TCAR" => new SteelTorsion_Tube(input),

                "IPE" or "HEA" or "PRS" => new SteelTorsion_IPEHEA(input),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
