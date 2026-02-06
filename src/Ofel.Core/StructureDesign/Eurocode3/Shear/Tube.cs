using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelShear_Y_Tube : SteelShear
    {
        private readonly SteelShearInput _input;
        public SteelShear_Y_Tube(SteelShearInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Y;
        public override double VEd => _input.Context.Effort.Fy;
        public override double A => _input.Context.Section.A_z;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public override double RhoTorsion => this.GetRhoTorsion();
        public override double VRd => this.GetVrd();
        public override double Rho => this.GetRho(_input);

        public override double GetRhoTorsion()
        {
            var torsion_input = (SteelTorsion_Tube)_input.Torsion;
            double value = 1 - torsion_input.Tau_t / (torsion_input.Fy / (Math.Sqrt(3) * torsion_input.GammaM0));
            return Math.Sqrt(value);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(VEd).SetName("V_y_Ed"),
                    new AreaData(A).SetName("A_v"),
                    new ResistanceData(Fy).SetName("f_y"),
                };
            var outputs = new List<IDataType>
                {
                    new ForceData(VRd).SetName("V_Rd"),
                    new CoefficientData(Ratio).SetName("%")
                };
            return Verification.Create("Shear Resistance Y Axis", inputs, outputs);
        }
    }

    public sealed class SteelShear_Z_Tube : SteelShear
    {
        private readonly SteelShearInput _input;
        public SteelShear_Z_Tube(SteelShearInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Z;
        public override double VEd => _input.Context.Effort.Fz;
        public override double A => _input.Context.Section.A_y;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public override double RhoTorsion => this.GetRhoTorsion();
        public override double VRd => this.GetVrd();
        public override double Rho => this.GetRho(_input);

        public override double GetRhoTorsion()
        {
            var torsion_input = (SteelTorsion_Tube)_input.Torsion;
            double value = 1 - torsion_input.Tau_t / (torsion_input.Fy / (Math.Pow(3, 0.5) * torsion_input.GammaM0));
            return Math.Sqrt(value);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(VEd).SetName("V_y_Ed"),
                    new AreaData(A).SetName("A_v"),
                    new ResistanceData(Fy).SetName("f_y"),
                };
            var outputs = new List<IDataType>
                {
                    new ForceData(VRd).SetName("V_Rd"),
                    new CoefficientData(Ratio).SetName("%")
                };
            return Verification.Create("Shear Resistance Y Axis", inputs, outputs);
        }
    }

}
