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
    public sealed class SteelBucklingTraction : SteelBuckling
    {
        private readonly SteelBucklingDefaultInput _input;

        public override double Ncr { get; } = double.MaxValue;
        public override double LambdaBar { get; } = 0.0;
        public override double Chi { get; } = 1.0;
        public override double NEd => _input.Context.Effort.Fx;
        public override double NbRd => _input.Context.Section.A * _input.Context.Material.Fy / _input.Context.Coef.GammaM1;
        public SteelBucklingTraction(SteelBucklingDefaultInput input)
        {
            _input = input;
        }


        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Propriétés de section
                new AreaData(_input.Context.Section.A).SetName("A"),

                // Matériau
                new ResistanceData(_input.Context.Material.Fy).SetName("f_y"),
            };

            var outputs = new List<IDataType>
            {
                // Grandeurs intermédiaires EC3
                new CoefficientData(Chi).SetName("χ"),
                new ForceData(NbRd).SetName("N_b,Rd"),
            };

            return Verification.Create(
                "Buckling Resistance – Axial Compression (EC3)",
                inputs,
                outputs
            );
        }
    }
}
