using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ofel.Core.StructureDesign.Eurocode3.SteelBucklingFactory;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelBucklingDefaultInput : ISteelBucklingInput
    {
        public BucklingType Type => BucklingType.Default;
        public required SteelVerificationContext Context { get; init; }
        public required Axis Axis { get; init; }
        public required IntervalBuckling Interval { get; set; }

    }

    public sealed class SteelBucklingDefault : SteelBuckling
    {
        private readonly SteelBucklingDefaultInput _input;

        public BucklingCurve Curve { get; }
        public double Alpha { get; }
        public double L_cr { get; }
        public override double Ncr { get; }
        public override double LambdaBar { get; }
        public double Phi { get; }
        public override double NEd => _input.Context.Effort.Fx;
        public override double NbRd => Chi * _input.Context.Section.A * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;
        public override double Chi { get; }
        public SteelBucklingDefault(SteelBucklingDefaultInput input)
        {
            _input = input;
            L_cr = input.Interval.Length * input.Interval.K;
            Ncr = this.GetNcr(input.Context.Material.E, input.Context.Section.GetInertia(input.Axis), L_cr);
            LambdaBar = this.GetLambdaBar(Ncr, input.Context.Section.A, input.Context.Material.Fy, input.Context.Coef.GammaM1);
            Curve = input.Context.Section.GetBucklingCurve(input.Axis);
            Alpha = this.GetAlpha(Curve);
            Phi = this.GetPhi(LambdaBar, Alpha);
            Chi = GetChi(Phi, LambdaBar);
        }


        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Géométrie / stabilité
                new LengthData(L_cr).SetName("L_cr"),

                // Propriétés de section
                new AreaData(_input.Context.Section.A).SetName("A"),
                new InertiaData(_input.Context.Section.GetInertia(_input.Axis)).SetName("I"),

                // Matériau
                new ResistanceData(_input.Context.Material.Fy).SetName("f_y"),
                new ResistanceData(_input.Context.Material.E).SetName("E"),

                // Courbe de flambement
                new TextData(Curve.ToString()).SetName("Curve")
            };

            var outputs = new List<IDataType>
            {
                // Grandeurs intermédiaires EC3
                new ForceData(Ncr).SetName("N_cr"),
                new CoefficientData(LambdaBar).SetName("λ̄"),
                new CoefficientData(Alpha).SetName("α"),
                new CoefficientData(Phi).SetName("φ"),
                new CoefficientData(Chi).SetName("χ"),
            };

            return Verification.Create(
                "Buckling Resistance – Axial Compression (EC3)",
                inputs,
                outputs
            );
        }
    }
}
