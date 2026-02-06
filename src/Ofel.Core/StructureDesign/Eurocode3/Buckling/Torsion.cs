using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Ofel.Core.StructureDesign.Eurocode3.SteelBucklingFactory;


namespace Ofel.Core.StructureDesign.Eurocode3
{
    public interface ISteelBucklingTorsion
    {
        public double Ncrt { get; }
        public double NcrTfz { get; }
        public double NcrTfy { get; }
        public double I0 { get; }
    }


    public abstract class SteelBucklingTorsionBase
    : SteelBuckling, ISteelBucklingTorsion
    {
        private readonly SteelBucklingTorsionInput _input;

        public double I0 { get; }
        public double NcrTfz { get; protected set; }
        public double NcrTfy { get; protected set; }
        public double Ncrt { get; protected set; }
        public override double NEd => _input.Context.Effort.Fx;

        protected SteelBucklingTorsionBase(SteelBucklingTorsionInput input)
        {
            _input = input;
            I0 = input.Context.Section.GetI0();
        }
    }


    public sealed class SteelBucklingTorsionInput : ISteelBucklingInput
    {
        public BucklingType Type => BucklingType.Torsion;
        public required SteelVerificationContext Context { get; init; }
        public required IntervalBuckling Interval { get; set; }
        public required SteelBuckling BucklingY { get; init; }
        public required SteelBuckling BucklingZ { get; init; }
    }

    public sealed class SteelBucklingTorsion : SteelBucklingTorsionBase
    {
        private readonly SteelBucklingTorsionInput _input;
        public double L_cr { get; }
        public override double NEd => _input.Context.Effort.Fx;
        public override double Ncr { get; }
        public BucklingCurve Curve { get; }
        public double Alpha { get; }
        public override double LambdaBar { get; }
        public double Phi { get; }
        public override double Chi { get; }
        public override double NbRd => Chi * _input.Context.Section.A * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;

        public SteelBucklingTorsion(SteelBucklingTorsionInput input) : base(input)
        {
            _input = input;
            L_cr = input.Interval.Length * input.Interval.K;
            Ncrt = this.GetNcrt();
            NcrTfy = this.GetNcrTf(_input.BucklingY.Ncr);
            NcrTfz = this.GetNcrTf(_input.BucklingZ.Ncr);
            Ncr = Math.Min(NcrTfy, NcrTfz);
            Curve = input.Context.Section.GetBucklingCurve(Axis.Z);
            Alpha = this.GetAlpha(Curve);
            LambdaBar = this.GetLambdaBar(Ncr, input.Context.Section.A, input.Context.Material.Fy, input.Context.Coef.GammaM1);
            Phi = this.GetPhi(LambdaBar, Alpha);
            Chi = this.GetChi(Phi, LambdaBar);
        }
        public double GetNcrTf(double ncr)
        {
            var Iy = _input.Context.Section.I_y;
            var Iz = _input.Context.Section.I_z;
            var a = ncr + Ncrt;
            var b = 4 * ncr * Ncrt * (Iy + Iz) / I0;

            var discriminant = a * a - b;

            if (discriminant < 0)
                throw new InvalidOperationException("Negative discriminant in Ncr computation.");

            var root = Math.Sqrt(discriminant);

            var denominator = 2 * (Iy + Iz);

            return I0 / denominator * (a - root);
        }

        public double GetNcrt()
        {
            double A = _input.Context.Section.A;
            double G = _input.Context.Material.G;
            double E = _input.Context.Material.E;
            double It = _input.Context.Section.I_t;
            double Iw = _input.Context.Section.GetIw();
            return A / I0 * (G * It + Math.Pow(Math.PI, 2) * E * Iw / Math.Pow(L_cr, 2));
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Géométrie / stabilité
                new LengthData(L_cr).SetName("L_cr"),

                // Propriétés de section
                new AreaData(_input.Context.Section.A).SetName("A"),
                new InertiaData(_input.Context.Section.I_y).SetName("I_y"),
                new InertiaData(_input.Context.Section.I_z).SetName("I_z"),
                new InertiaData(I0).SetName("I_0"),
                new InertiaData(_input.Context.Section.I_t).SetName("I_t"),
                new InertiaData(_input.Context.Section.GetIw()).SetName("I_w"),

                // Matériau
                new ResistanceData(_input.Context.Material.E).SetName("E"),
                new ResistanceData(_input.Context.Material.G).SetName("G"),

                // Effets de flambement simples
                new ForceData(_input.BucklingY.Ncr).SetName("N_cr,y"),
                new ForceData(_input.BucklingZ.Ncr).SetName("N_cr,z"),
            };

            var outputs = new List<IDataType>
            {
                // Flambement torsion pur
                new ForceData(Ncrt).SetName("N_cr,t"),

                // Flambement couplé Y–Z–torsion
                new ForceData(Ncr).SetName("N_cr_TF"),
            };

            return Verification.Create(
                "Buckling Resistance – Coupled Y–Z–Torsion (EC3)",
                inputs,
                outputs
            );
        }
    }
    public sealed class SteelBucklingTorsionTraction : SteelBucklingTorsionBase
    {
        private readonly SteelBucklingTorsionInput _input;
        public override double NEd => _input.Context.Effort.Fx;
        public override double Ncr { get; } = double.MaxValue;
        public override double LambdaBar { get; } = 0.0;
        public override double Chi { get; } = 1.0;
        public override double NbRd => Chi * _input.Context.Section.A * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;
        public SteelBucklingTorsionTraction(SteelBucklingTorsionInput input) : base(input)
        {
            _input = input;
            Ncrt = double.MaxValue;
            NcrTfy = double.MaxValue;
            NcrTfz = double.MaxValue;
        }
        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Géométrie / stabilité
                // Propriétés de section
                new InertiaData(_input.Context.Section.I_y).SetName("I_y"),
                new InertiaData(_input.Context.Section.I_z).SetName("I_z"),
                new InertiaData(I0).SetName("I_0"),
            };

            var outputs = new List<IDataType>
            {
                // Flambement torsion pur
                new ForceData(Ncrt).SetName("N_cr,t"),

                // Flambement couplé Y–Z–torsion
                new ForceData(Ncr).SetName("N_cr_TF"),
            };

            return Verification.Create(
                "Buckling Resistance – Coupled Y–Z–Torsion (EC3)",
                inputs,
                outputs
            );
        }
    }
}
