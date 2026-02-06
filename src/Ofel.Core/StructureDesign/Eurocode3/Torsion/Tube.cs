using Ofel.Core.Data;
using Ofel.Core.SectionParameter;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelTorsion_Tube : SteelTorsion
    {
        private readonly SteelTorsionInput _input;
        public SteelTorsion_Tube(SteelTorsionInput input)
        {
            _input = input;
        }
        public override double Mx => _input.Context.Effort.Mx;
        public override double I_t => _input.Context.Section.I_t;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public double Tau_t => this.GetTau_T(_input);
        public override double Ratio => this.GetRatio(_input);
        public override Verification ToVerification()
        {
            throw new NotImplementedException();
        }

        private double GetTau_T(SteelTorsionInput input)
        {
            return (Mx / (2 * _input.Context.Section.H * _input.Context.Section.B * _input.Context.Section.T_w));
        }

        private double GetRatio(SteelTorsionInput _input)
        {
            return Tau_t / (Fy / Math.Sqrt(3));
        }
    }
}
