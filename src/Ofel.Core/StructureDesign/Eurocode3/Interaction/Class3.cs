using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelInteraction_Class3 : SteelInteraction_3
    {
        private readonly SteelInteractionInput _input;
        public SteelInteraction_Class3(SteelInteractionInput input)
        {
            _input = input;
        }
        public override double Sigma_N => this.GetSigma_Ned();
        public override double Sigma_M_yEd => this.GetSigma_M_yEd();
        public override double Sigma_M_zEd => this.GetSigma_M_zEd();
        public override double Sigma => Math.Abs(Sigma_N) + Math.Abs(Sigma_M_yEd) + Math.Abs(Sigma_M_zEd);
        public override double Fy => _input.Context.Material.Fy;
        public override double Ratio => this.GetRatio();

        public double GetSigma_Ned()
        {
            return _input.Normal.N_Ed / _input.Context.Section.A;
        }
        public double GetSigma_M_yEd()
        {
            return _input.FlexionY.MEd * 0.5 * _input.Context.Section.H / _input.Context.Section.I_y;
        }
        public double GetSigma_M_zEd()
        {
            return _input.FlexionZ.MEd * 0.5 * _input.Context.Section.B / _input.Context.Section.I_z;
        }
        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Efforts appliqués
                new ForceData(_input.Normal.N_Ed).SetName("N_Ed"),
                new MomentData(_input.FlexionY.MEd).SetName("M_y_Ed"),
                new MomentData(_input.FlexionZ.MEd).SetName("M_z_Ed"),

                // Propriétés de section
                new AreaData(_input.Context.Section.A).SetName("A"),
                new LengthData(_input.Context.Section.H).SetName("H"),
                new LengthData(_input.Context.Section.B).SetName("B"),
                new InertiaData(_input.Context.Section.I_y).SetName("I_y"),
                new InertiaData(_input.Context.Section.I_z).SetName("I_z"),

                // Matériau
                new ResistanceData(Fy).SetName("f_y")
            };

            var outputs = new List<IDataType>
            {
                // Contraintes
                new ResistanceData(Sigma_N).SetName("σ_N"),
                new ResistanceData(Sigma_M_yEd).SetName("σ_My"),
                new ResistanceData(Sigma_M_zEd).SetName("σ_Mz"),
                new ResistanceData(Sigma).SetName("σ_eq"),

                // Critère final
                new CoefficientData(Ratio).SetName("Ratio")
            };

            return Verification.Create(
                "Steel Interaction – Class 3 (Stress Check)",
                inputs,
                outputs
            );
        }

    }
}
