using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public interface IInterval
    {
        public abstract double Start { get; }
        public abstract double End { get; }
        public abstract double Length { get; }

        public abstract Boolean Contains(double position);

    }

    public abstract class IntervalDisplacements : IInterval
    {
        public double Start { get; }
        public bool IsStartFixed { get; }
        public double End { get; }
        public bool IsEndFixed { get; }
        public double Length { get; set; }

        public Boolean Contains(double position)
        {
            return position >= Start && position <= End;
        }

    }

    public sealed class IntervalBuckling : IInterval
    {
        public double Start { get; init; }
        public double End { get; init; }
        public double K { get; init; }
        public double Length { get; init; }
        public Boolean Contains(double position)
        {
            return position >= Start && position <= End;
        }

    }

    public enum BucklingType
    {
        None,
        Torsion,
        Default,
        Traction
    }

    public interface ISteelBucklingInput
    {
        public BucklingType Type { get; }
        public SteelVerificationContext Context { get; }
    }


    public static class SteelBucklingFactory
    {
        public static SteelBuckling Create(ISteelBucklingInput input)
        {
            switch (input.Type)
            {
                case BucklingType.Torsion:
                    switch (input.Context.Effort.Fx)
                    {
                        case > 0:
                            return new SteelBucklingTorsion(
                                (SteelBucklingTorsionInput)input);

                        case <= 0:
                            return new SteelBucklingTorsionTraction(
                                (SteelBucklingTorsionInput)input);
                    }
                    break;

                case BucklingType.Default:
                    switch (input.Context.Effort.Fx)
                    {
                        case > 0:
                            return new SteelBucklingDefault(
                                (SteelBucklingDefaultInput)input);

                        case <= 0:
                            return new SteelBucklingTraction(
                                (SteelBucklingDefaultInput)input);
                    }
                    break;
            }

            throw new ArgumentOutOfRangeException(nameof(input));
        }
    }
    public abstract class SteelBuckling : IVerifiable
    {
        public abstract double Ncr { get; }
        public abstract double LambdaBar { get; }
        public abstract double Chi { get; }
        public abstract double NbRd { get; }
        public abstract double NEd { get; }
        public double Ratio => Math.Abs(NEd / NbRd);
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
            return 0.5 * (1 + alpha * (LambdaBar - lambda0) + Math.Pow(LambdaBar, 2));
        }
        public double GetChi(double phi, double lambdaBar)
        {
            return Math.Min(1 / (phi + Math.Sqrt(Math.Pow(phi, 2) - Math.Pow(lambdaBar, 2))), 1.0);
        }
        public double GetNcr(double E, double I, double Lb)
        {
            return Math.Pow(Math.PI, 2) * E * I / Math.Pow(Lb, 2);
        }
        public double GetLambdaBar(double Ncr, double A, double Fy, double gammaM1)
        {
            return Math.Sqrt(A * Fy / (Ncr * gammaM1));
        }
    }
}
