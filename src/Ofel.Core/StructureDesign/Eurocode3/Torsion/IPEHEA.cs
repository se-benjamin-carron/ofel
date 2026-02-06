using Ofel.Core.Data;
using Ofel.Core.SectionParameter;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelTorsion_IPEHEA : SteelTorsion
    {
        private readonly SteelTorsionInput _input;
        public SteelTorsion_IPEHEA(SteelTorsionInput input)
        {
            _input = input;
        }
        public override double Mx => _input.Context.Effort.Mx;
        public override double I_t => _input.Context.Section.I_t;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public double Tau_w => this.GetTau(_input.Context.Section.T_w);
        public double Tau_f => this.GetTau(_input.Context.Section.T_f);
        public double Ratio_w => this.GetRatio(Tau_w);
        public double Ratio_f => this.GetRatio(Tau_f);
        public override double Ratio => Math.Max(Ratio_w, Ratio_f);

        public override Verification ToVerification()
        {
            throw new NotImplementedException();
        }
        private double GetTau(double thickness)
        {
            return (Math.Abs(Mx) / I_t) * thickness;
        }
        private double GetRatio(double stress)
        {
            return Math.Abs(stress) / (Fy / Math.Sqrt(3));
        }
    }
}
