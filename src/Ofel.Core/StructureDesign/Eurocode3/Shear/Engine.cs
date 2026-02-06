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
    public sealed class SteelShearInput()
    {
        public required Axis Axis { get; init; }
        public required SteelVerificationContext Context { get; init; }
        public required SteelTorsion Torsion { get; init; }
    }

    public abstract class SteelShear : IVerifiable
    {
        public abstract Axis Axis { get; }
        public abstract double A { get; }
        public abstract double Fy { get; }
        public abstract double GammaM0 { get; }
        public abstract double VEd { get; }
        public abstract double VRd { get; }
        public abstract double Rho { get; }
        public abstract double RhoTorsion { get; }
        public double Ratio => Math.Abs(VEd / VRd);
        public abstract Verification ToVerification();
        public abstract double GetRhoTorsion();
        public double GetRho(SteelShearInput _input)
        {
            return Math.Pow(Math.Max(2 * Math.Abs(VEd) / VRd - 1, 0.0), 2);
        }
        public double GetVrd()
        {
            return RhoTorsion * A * Fy / (Math.Sqrt(3) * GammaM0);
        }
    }

    public static class SteelShearFactory
    {
        public static SteelShear Create(SteelShearInput input)
        {
            return (input.Axis, input.Context.Section.ProfileType) switch
            {
                (Axis.Y, "HEA" or "IPE" or "PRS") => new SteelShear_Y_IPEHEA(input),
                (Axis.Z, "HEA" or "IPE" or "PRS") => new SteelShear_Z_IPEHEA(input),
                (Axis.Y, "TCAR") => new SteelShear_Y_Tube(input),
                (Axis.Z, "TCAR") => new SteelShear_Z_Tube(input),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
