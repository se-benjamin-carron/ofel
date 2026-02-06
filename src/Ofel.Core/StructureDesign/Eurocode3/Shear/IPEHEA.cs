using MathNet.Numerics.Distributions;
using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelShear_Y_IPEHEA : SteelShear
    {
        private readonly SteelShearInput _input;
        public SteelShear_Y_IPEHEA(SteelShearInput input)
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
            var torsion_input = (SteelTorsion_IPEHEA)_input.Torsion;
            double value = 1 - torsion_input.Tau_f / (1.25 * torsion_input.Fy / (Math.Pow(3, 0.5) * torsion_input.GammaM0));
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

    public sealed class SteelShear_Z_IPEHEA : SteelShear
    {
        private readonly SteelShearInput _input;
        public SteelShear_Z_IPEHEA(SteelShearInput input)
        {
            _input = input;
            ChiV = this.GetChiV();
        }
        public override Axis Axis => Axis.Z;
        public override double VEd => _input.Context.Effort.Fz;
        public override double A => _input.Context.Section.A_y;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;
        public override double RhoTorsion => this.GetRhoTorsion();
        public override double VRd => this.GetVrd();
        public override double Rho => this.GetRho(_input);
        public double ChiV { get; }
        public override double GetRhoTorsion()
        {
            var torsion_input = (SteelTorsion_IPEHEA)_input.Torsion;
            double value = 1 - Math.Abs(torsion_input.Tau_w) / (1.25 * torsion_input.Fy / (Math.Pow(3, 0.5) * torsion_input.GammaM0));
            return Math.Sqrt(value);
        }
        public double GetChiV()
        {
            double epsilon = _input.Context.Material.GetEpsilon();
            double hw = _input.Context.Section.H - 2 * _input.Context.Section.T_f - 2 * _input.Context.Section.R_1;
            double tw = _input.Context.Section.T_w;
            return Math.Min(1.0, 72 * epsilon / (hw / tw));
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
                    new CoefficientData(ChiV).SetName("χ_V"),
                    new ForceData(VRd).SetName("V_Rd"),
                    new CoefficientData(Ratio).SetName("%")
                };
            return Verification.Create("Shear Resistance Y Axis", inputs, outputs);
        }
        new public double GetVrd()
        {
            return ChiV * RhoTorsion * A * Fy / (Math.Sqrt(3) * GammaM0);

        }
    }

}
