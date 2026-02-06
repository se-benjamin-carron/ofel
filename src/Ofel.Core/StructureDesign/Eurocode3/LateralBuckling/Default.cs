using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelLateralBucklingDefault : SteelLateralBuckling
    {
        private readonly SteelLateralBucklingInput _input;
        public BucklingCurve Curve { get; }
        public double Alpha { get; }
        public double L_cr { get; }
        public override double Mcr { get; }
        public override double LambdaBar { get; }
        public override double Lambda0 { get; }
        public double LambdaLT0 { get; }
        public double Phi { get; }
        public override double Chi { get; }
        public override double MEd => _input.Context.Effort.My;
        public override double MbRd => Chi * _input.Context.Section.GetInertiaModulus(Axis.Y, _input.Context.SectionClass) * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;
        public SteelLateralBucklingDefault(SteelLateralBucklingInput input)
        {
            _input = input;
            L_cr = input.Interval.Length * input.Interval.K;
            Mcr = this.GetMcr(input);
            LambdaBar = this.GetLambdaBar(Mcr, input.Context.Section.GetInertiaModulus(Axis.Y, input.Context.SectionClass), input.Context.Material.Fy, input.Context.Coef.GammaM1);
            Lambda0 = GetLambda0(input.Context.Material, input.Context.Section, input.Interval.GetLength(), input.Context.SectionClass);
            Alpha = _input.Context.Section.GetAlphaLT0(LambdaBar);
            LambdaLT0 = _input.Context.Section.GetLambdaLT0();
            Phi = this.GetPhi(LambdaBar, Alpha, LambdaLT0);
            Chi = this.GetChi(Phi, LambdaBar);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Géométrie / stabilité
                new LengthData(L_cr).SetName("L_cr"),

                // Propriétés de section
                new AreaData(_input.Context.Section.GetInertiaModulus(Axis.Y, _input.Context.SectionClass)).SetName("W_y"),
                new InertiaData(_input.Context.Section.I_t).SetName("I_t"),
                new InertiaData(_input.Context.Section.GetIw()).SetName("I_w"),

                // Matériau
                new ResistanceData(_input.Context.Material.Fy).SetName("f_y"),
                new ResistanceData(_input.Context.Material.E).SetName("E"),

                // Courbe de flambement latéral
                new TextData(Curve.ToString()).SetName("Curve"),
                new TextData(_input.CoefficientC1C2.C1.ToString()).SetName("C1"),
                new TextData(_input.CoefficientC1C2.C2.ToString()).SetName("C2")

            };

            var outputs = new List<IDataType>
            {
                // Grandeurs calculées
                new ForceData(Mcr).SetName("Mcr"),
                new CoefficientData(LambdaBar).SetName("λ̄"),
                new CoefficientData(Alpha).SetName("α"),
                new CoefficientData(Phi).SetName("φ"),
                new CoefficientData(Chi).SetName("χ"),
            };

            return Verification.Create(
                "Lateral Buckling – Flexural Buckling (EC3)",
                inputs,
                outputs
            );
        }
    }
}
