using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelNormal_Stable : SteelNormal
    {
        private readonly SteelNormalInput _input;
        public SteelNormal_Stable(SteelNormalInput input)
        {
            _input = input;
        }

        public override int SectionClass => _input.Context.SectionClass.DesignClass;

        public override double A => _input.Context.Section.A;
        public override double N_Ed => _input.Context.Effort.Fx;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(N_Ed).SetName("N_Ed"),
                    new AreaData(A).SetName("A"),
                    new ResistanceData(Fy).SetName("f_y"),
                };
            var outputs = new List<IDataType>
                {
                    new ForceData(N_Rd).SetName("N_Rd"),
                    new CoefficientData(Ratio).SetName("%")
                };
            return Verification.Create("Normal Stable Verification Class", inputs, outputs);
        }
    }
    public sealed class SteelNormal_InstableIPE : SteelNormal
    {
        private readonly SteelNormalInput _input;
        public SteelNormal_InstableIPE(SteelNormalInput input)
        {
            _input = input;
        }

        public override int SectionClass => _input.Context.SectionClass.DesignClass;

        public override double A => _input.Context.Section.A;
        public override double N_Ed => _input.Context.Effort.Fx;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(N_Ed).SetName("N_Ed"),
                    new AreaData(A).SetName("A"),
                    new ResistanceData(Fy).SetName("f_y"),
                };
            var outputs = new List<IDataType>
                {
                    new ForceData(N_Rd).SetName("N_Rd"),
                    new CoefficientData(Ratio).SetName("%")
                };
            return Verification.Create("Normal Stable Verification Class", inputs, outputs);
        }
    }
}
