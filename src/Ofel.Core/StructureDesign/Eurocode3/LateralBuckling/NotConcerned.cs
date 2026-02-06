using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelLateralBucklingNotConcerned : SteelLateralBuckling
    {
        private readonly SteelLateralBucklingInput _input;
        public override double Mcr => double.MaxValue;
        public override double LambdaBar => double.MaxValue;
        public override double Lambda0 => double.MaxValue;
        public override double Chi => 1.0;
        public override double MEd => _input.Context.Effort.My;
        public override double MbRd => _input.Context.Section.GetInertiaModulus(Axis.Y, _input.Context.SectionClass) * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;
        public SteelLateralBucklingNotConcerned(SteelLateralBucklingInput input)
        {
            _input = input;
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
            };

            var outputs = new List<IDataType>
            {
                // Grandeurs calculées
                new TextData("NotConcerned Because Tubular Section").SetName("Status"),
                new ForceData(Mcr).SetName("Mcr"),
                new CoefficientData(LambdaBar).SetName("λ̄"),
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
