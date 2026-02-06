using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelInteraction_TCAR : SteelInteraction_12
    {
        private readonly SteelInteractionInput _input;
        public SteelInteraction_TCAR(SteelInteractionInput input)
        {
            _input = input;
        }
        public override double n => _input.Normal.Ratio;
        public double alpha_f => this.GetA(_input.Context.Section.A, _input.Context.Section.T_w, _input.Context.Section.B);
        public double alpha_w => this.GetA(_input.Context.Section.A, _input.Context.Section.T_f, _input.Context.Section.B);
        public override double alpha => this.GetAlpha();
        public override double beta => this.GetAlpha();

        public double Fy => _input.Context.Material.Fy;
        public double GammaM0 => _input.Context.Coef.GammaM0;
        public override double M_N_Y_Rd => this.GetM_N_Y_Rd();
        public override double M_N_Z_Rd => this.GetM_N_Z_Rd();

        public override double Ratio => this.GetRatio(_input);

        public double GetA(double A, double t_w, double B)
        {
            return (A - 2 * B * t_w) / A;
        }
        public double GetAlpha()
        {
            return Math.Min(6, 1.66 / (1 - 1.13 * Math.Pow(n, 2)));
        }
        public double GetM_N_Y_Rd()
        {
            return _input.FlexionY.MRd * (1 - n) / (1 - 0.5 * alpha_w);
        }
        public double GetM_N_Z_Rd()
        {
            return _input.FlexionZ.MRd * (1 - n) / (1 - 0.5 * alpha_f);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Effort normal
                new ForceData(_input.Normal.N_Ed).SetName("N_Ed"),

                // Moments appliqués
                new MomentData(_input.FlexionY.MEd).SetName("M_y_Ed"),
                new MomentData(_input.FlexionZ.MEd).SetName("M_z_Ed"),

                // Paramètres matériaux
                new ResistanceData(Fy).SetName("f_y"),
                new CoefficientData(GammaM0).SetName("γ_M0"),
            };

            var outputs = new List<IDataType>
            {
                // Coefficients d’interaction
                new CoefficientData(alpha).SetName("α"),
                new CoefficientData(beta).SetName("β"),
                new CoefficientData(n).SetName("n"),
                // Résistances réduites
                new MomentData(M_N_Y_Rd).SetName("M_N_y_Rd"),
                new MomentData(M_N_Z_Rd).SetName("M_N_z_Rd"),

                // Critère d’interaction
                new CoefficientData(Ratio).SetName("Ratio")
            };

            return Verification.Create(
                "Steel Interaction N-My-Mz (Class 1-2 – TCAR)",
                inputs,
                outputs
            );
        }

    }
}
