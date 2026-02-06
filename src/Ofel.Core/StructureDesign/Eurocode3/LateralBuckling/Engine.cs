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

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class IntervalLateralBuckling : IInterval
    {
        public double Start { get; }
        public double End { get; }
        public double K { get; }
        public double K_w { get; }
        public double K_z { get; }
        public double Length { get; set; }
        public double GetLength()
        {
            return Length * K;
        }

        public Boolean Contains(double position)
        {
            return position >= Start && position <= End;
        }

        public IntervalLateralBuckling(double length, double k, double k_w = 1, double k_z = 1, double en = 0, double st = 1.0)
        {
            Length = length;
            K = k;
            K_w = k_w;
            K_z = k_z;
            Start = st;
            End = en;
        }

    }

    public enum StabilisingForceType
    {
        Stabilising,
        Neutral,
        Destabilising,
    }

    public enum LateralBucklingType
    {
        Default,
        CorrectedLateralBuckling,
        NotConcerned,
    }

    public enum CoefficientC1C2Type
    {
        LinearShape,
        Default,
    }

    public class FlexionCoefficient
    {
        public double C1 { get; init; }
        public double C2 { get; init; }
        public CoefficientC1C2Type Type { get; init; }
        private FlexionCoefficient(double c1, double c2, CoefficientC1C2Type type)
        {
            C1 = c1;
            C2 = c2;
            Type = type;
        }

        public static FlexionCoefficient CreateFromLinearShape(double ratio)
        {
            double c1 = CreateC1FromRatio(ratio);
            double c2 = 0.0;
            return new FlexionCoefficient(c1, c2, CoefficientC1C2Type.LinearShape);

        }
        public static FlexionCoefficient CreateFromDefault(double c1, double c2)
        {
            return new FlexionCoefficient(c1, c2, CoefficientC1C2Type.Default);
        }

        private static double CreateC1FromRatio(double ratio)
        {
            return 1.0 / Math.Sqrt(0.325 + 0.423 * ratio + 0.252 * Math.Pow(ratio, 2));
        }
    }

    public class SteelLateralBucklingInput
    {
        public required LateralBucklingType Type { get; init; }
        public required FlexionCoefficient CoefficientC1C2 { get; init; }
        public required IntervalLateralBuckling Interval { get; init; }
        public required SteelVerificationContext Context { get; init; }
        public required StabilisingForceType IsStabilisingForce { get; init; }
    }


    public static class SteelLateralBucklingFactory
    {
        public static SteelLateralBuckling Create(SteelLateralBucklingInput input)
        {
            return (input.Type) switch
            {
                LateralBucklingType.Default => new SteelLateralBucklingDefault(input),
                LateralBucklingType.CorrectedLateralBuckling => new SteelLateralBucklingCorrected(input),
                LateralBucklingType.NotConcerned => new SteelLateralBucklingNotConcerned(input),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public abstract class SteelLateralBuckling : IVerifiable
    {
        public abstract double Mcr { get; }
        public abstract double Chi { get; }
        public abstract double LambdaBar { get; }
        public abstract double Lambda0 { get; }
        public abstract double MbRd { get; }
        public abstract double MEd { get; }
        public double Ratio => Math.Abs(MEd / MbRd);

        public abstract Verification ToVerification();
        public double GetAlpha(BucklingCurve curve)
        {
            switch (curve)
            {
                case BucklingCurve._A: return 0.21;
                case BucklingCurve._B: return 0.34;
                case BucklingCurve._C: return 0.49;
                case BucklingCurve._D: return 0.76;
                default: throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }
        }
        public double GetPhi(double lambdaBar, double alpha, double lambda0 = 0.2)
        {
            return 0.5 * (1 + alpha * (lambdaBar - lambda0) + Math.Pow(lambdaBar, 2));
        }
        public double GetChi(double phi, double lambdaBar)
        {
            return Math.Min(1 / (phi + Math.Sqrt(Math.Pow(phi, 2) - Math.Pow(lambdaBar, 2))), 1.0);
        }
        public double GetNcr(double E, double I, double Lb)
        {
            return Math.Pow(Math.PI, 2) * E * I / Math.Pow(Lb, 2);
        }
        public double GetLambdaBar(double Mcr, double Wy, double Fy, double gammaM1)
        {
            return Math.Sqrt(Wy * Fy / (Mcr * gammaM1));
        }
        public double GetMcr(SteelLateralBucklingInput input)
        {
            double E = input.Context.Material.E;
            double G = input.Context.Material.G;
            double I_z = input.Context.Section.GetInertia(Axis.Z);
            double I_y = input.Context.Section.GetInertia(Axis.Y);
            double Lb = input.Interval.Length * input.Interval.K;
            double k_z = input.Interval.K_z;
            double k_w = input.Interval.K_w;
            double I_t = input.Context.Section.I_t;
            double I_w = input.Context.Section.I_w;
            double c1 = input.CoefficientC1C2.C1;
            double c2 = input.CoefficientC1C2.C2;
            double zg = this.GetZg(input.IsStabilisingForce, input.Context.Section.H);
            double piSquared = Math.Pow(Math.PI, 2);

            double part1 = c1 * (piSquared * E * I_z) / Math.Pow(Lb * k_z, 2);
            double part2 = Math.Pow(k_z / k_w, 2) * I_w / I_z;
            double part3 = Math.Pow(k_z * Lb, 2) * (G * I_t) / (piSquared * E * I_z);
            double part4 = c2 * zg;
            return part1 * (Math.Sqrt(part2 + part3 + part4 * part4) - part4);
        }

        public double GetMcr0(SteelMaterial material, SteelSection section, double L)
        {
            double E = material.E;
            double G = material.G;
            double I_z = section.GetInertia(Axis.Z);
            double I_y = section.GetInertia(Axis.Y);
            double I_t = section.I_t;
            double I_w = section.I_w;
            double zg = this.GetZg(StabilisingForceType.Neutral, section.H);
            double piSquared = Math.Pow(Math.PI, 2);

            double part1 = (piSquared * E * I_z) / Math.Pow(L, 2);
            double part2 = I_w / I_z;
            double part3 = Math.Pow(L, 2) * (G * I_t) / (piSquared * E * I_z);
            return part1 * (Math.Sqrt(part2 + part3));
        }

        public double GetLambda0(SteelMaterial material, SteelSection section, double L, ClassSection classSection)
        {
            double Mcr0 = this.GetMcr0(material, section, L);
            double W_y = section.GetInertiaModulusByInt(Axis.Y, classSection.DesignClass);
            double Fy = material.Fy;
            return Math.Sqrt(W_y * Fy / Mcr0);

        }

        public double GetZg(StabilisingForceType IsStabilising, double H)
        {
            switch (IsStabilising)
            {
                case StabilisingForceType.Stabilising:
                    return -0.5 * H;
                case StabilisingForceType.Neutral:
                    return 0.0;
                case StabilisingForceType.Destabilising:
                    return 0.5 * H;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
