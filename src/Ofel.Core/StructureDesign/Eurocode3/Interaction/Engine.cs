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
    public sealed class SteelInteractionInput()
    {
        public required SteelVerificationContext Context { get; init; }
        public required SteelFlexion FlexionY { get; init; }
        public required SteelFlexion FlexionZ { get; init; }
        public required SteelNormal Normal { get; init; }
    }


    public abstract class SteelInteraction : IVerifiable
    {
        public abstract double Ratio { get; }
        public abstract Verification ToVerification();
    }
    public abstract class SteelInteraction_12 : SteelInteraction, IVerifiable
    {
        public abstract double n { get; }
        public abstract double M_N_Y_Rd { get; }
        public abstract double M_N_Z_Rd { get; }
        public abstract double alpha { get; }
        public abstract double beta { get; }
        public double GetRatio(SteelInteractionInput input)
        {
            double ratio_y = Math.Pow(input.FlexionY.MEd / M_N_Y_Rd, alpha);
            double ratio_z = Math.Pow(input.FlexionZ.MEd / M_N_Z_Rd, beta);
            return ratio_y + ratio_z;
        }
    }

    public abstract class SteelInteraction_3 : SteelInteraction, IVerifiable
    {
        public abstract double Sigma { get; }
        public abstract double Sigma_N { get; }
        public abstract double Sigma_M_yEd { get; }
        public abstract double Sigma_M_zEd { get; }
        public abstract double Fy { get; }
        public double GetRatio()
        {
            return Sigma / Fy;
        }
    }

    public static class SteelInteractionFactory
    {
        public static SteelInteraction Create(SteelInteractionInput input)
        {
            return (input.Context.Section.ProfileType, input.Context.SectionClass.DesignClass) switch
            {
                ("HEA" or "IPE" or "PRS", 1 or 2) => new SteelInteraction_IPEHEA(input),
                ("TCAR", 1 or 2) => new SteelInteraction_TCAR(input),
                (_, 3) => new SteelInteraction_Class3(input),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
