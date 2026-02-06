using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ofel.Core.StructureDesign.Eurocode3.ShapeCoefficient;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelInteractionInstabilities_AnnexA_3 : SteelInteractionInstabilities
    {
        private readonly SteelInteractionInstabilitiesInput _input;

        public double Mu_y { get; }
        public double Mu_z { get; }
        public double W_y { get; }
        public double W_z { get; }
        public double LambdaMax { get; }
        public double LambdaMin { get; }
        public double Lambda0 { get; }
        public double Epsilon_y { get; }
        public double A_LT { get; }
        public double Cmy { get; }
        public double Cmz { get; }
        public double CmLT = 1.0;
        public double Kyy { get; }
        public double Kyz { get; }
        public double Kzy { get; }
        public double Kzz { get; }

        public override double RatioY { get; }
        public override double RatioZ { get; }

        public SteelInteractionInstabilities_AnnexA_3(SteelInteractionInstabilitiesInput input)
        {
            _input = input;
            W_y = GetW(Axis.Y);
            W_z = GetW(Axis.Z);
            Mu_y = GetMu(Axis.Y);
            Mu_z = GetMu(Axis.Z);
            LambdaMax = Math.Max(input.BucklingZ.LambdaBar, input.BucklingY.LambdaBar);
            LambdaMin = GetLambdaMin();
            Lambda0 = _input.LateralBuckling.Lambda0;
            Epsilon_y = GetEpsilonY();
            A_LT = GetA_LT();
            Cmy = GetCmy();
            Cmz = GetCmz();
            CmLT = GetCmLT();
            Kyy = GetKyy();
            Kyz = GetKyz();
            Kzy = GetKzy();
            Kzz = GetKzz();

            RatioY = GetRatioY();
            RatioZ = GetRatioZ();
        }

        private double GetW(Axis Axis)
        {
            double w_pl = _input.Context.Section.GetInertiaModulusByInt(Axis, 1);
            double w_el = _input.Context.Section.GetInertiaModulusByInt(Axis, 3);
            return Math.Min(w_pl / w_el, 1.5);
        }

        private double GetMu(Axis axis)
        {
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncr = axis == Axis.Y ? _input.BucklingY.Ncr : _input.BucklingZ.Ncr;
            double chi = axis == Axis.Y ? _input.BucklingY.Chi : _input.BucklingZ.Chi;
            double up = 1 - Ned / Ncr;
            double inf = 1 - chi * Ned / Ncr;
            return up / inf;
        }

        public double GetLambdaMin()
        {
            double c1 = _input.CoefficientC1C2.C1;
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncrz = _input.BucklingZ.Ncr;
            double NcrTf = _input.Torsion.Ncr;
            return 0.2 * Math.Sqrt(c1) * Math.Pow((1 - Ned / Ncrz) * (1 - Ned / NcrTf), 0.25);
        }
        public double GetEpsilonY()
        {
            double m_y_ed = Math.Abs(_input.Context.Effort.My);
            double W_y = _input.Context.Section.GetInertiaModulusByInt(Axis.Y, 3);
            double A = _input.Context.Section.A;
            double n_ed = Math.Abs(_input.Context.Effort.Fx);
            return m_y_ed * A / (n_ed * W_y);
        }
        public double GetA_LT()
        {
            double i_t = _input.Context.Section.I_t;
            double i_y = _input.Context.Section.I_y;
            return 1 - (i_t / i_y);
        }

        public double GetCmy()
        {
            if (Lambda0 <= LambdaMin)
            {
                return _input.Cmi0.Cmy0;
            }
            else
            {
                double cmy0 = _input.Cmi0.Cmy0;
                return cmy0 + (1 - cmy0) * Math.Sqrt(Epsilon_y) * A_LT / (1 + Math.Sqrt(Epsilon_y) * A_LT);
            }
        }
        public double GetCmz()
        {
            if (Lambda0 <= LambdaMin)
            {
                return _input.Cmi0.Cmz0;
            }
            else
            {
                return _input.Cmi0.Cmz0;
            }
        }

        public double GetCmLT()
        {
            if (Lambda0 <= LambdaMin)
            {
                return _input.Cmi0.Cmy0;
            }
            else
            {
                double cmy0 = _input.Cmi0.Cmy0;
                double Ned = Math.Abs(_input.Context.Effort.Fx);
                double Ncrz = _input.BucklingZ.Ncr;
                double Ncrt = _input.Torsion.Ncrt;
                return Math.Max(Cmy * A_LT / Math.Sqrt((1 - Ned / Ncrz) * (1 - Ned / Ncrt)), 1.0);
            }
        }

        public double GetKyy()
        {
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncry = _input.BucklingY.Ncr;
            return Cmy * CmLT * Mu_y / (1 - Ned / Ncry);
        }
        public double GetKyz()
        {
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncrz = _input.BucklingZ.Ncr;
            return Cmz * Mu_y / (1 - Ned / Ncrz);
        }
        public double GetKzy()
        {
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncry = _input.BucklingY.Ncr;
            return Cmy * CmLT * Mu_z / (1 - Ned / Ncry);
        }

        public double GetKzz()
        {
            double Ned = Math.Abs(_input.Context.Effort.Fx);
            double Ncrz = _input.BucklingZ.Ncr;
            return Cmz * Mu_z / (1 - Ned / Ncrz);
        }
        public double GetRatioY()
        {
            double n_ed = Math.Abs(_input.Context.Effort.Fx);
            double n_rd = _input.Normal.N_Rd;
            double chi_y = _input.BucklingY.Chi;
            double m_y_ed = Math.Abs(_input.Context.Effort.My);
            double m_pl_y_rd = _input.FlexionY.MRd;
            double chi_lt = _input.LateralBuckling.Chi;
            double m_z_ed = Math.Abs(_input.Context.Effort.Mz);
            double m_pl_z_rd = _input.FlexionZ.MRd;
            double result = n_ed / (chi_y * n_rd) + (Kyy * m_y_ed / (chi_lt * m_pl_y_rd)) + (Kyz * m_z_ed / m_pl_z_rd);
            return result;
        }

        public double GetRatioZ()
        {
            double n_ed = Math.Abs(_input.Context.Effort.Fx);
            double n_rd = _input.Normal.N_Rd;
            double chi_z = _input.BucklingZ.Chi;
            double m_y_ed = Math.Abs(_input.Context.Effort.My);
            double m_pl_y_rd = _input.FlexionY.MRd;
            double chi_lt = _input.LateralBuckling.Chi;
            double m_z_ed = Math.Abs(_input.Context.Effort.Mz);
            double m_pl_z_rd = _input.FlexionZ.MRd;
            double result = n_ed / (chi_z * n_rd) + (Kzy * m_y_ed / (chi_lt * m_pl_y_rd)) + (Kzz * m_z_ed / m_pl_z_rd);
            return result;
        }
        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
            {
                // Efforts
                new ForceData(_input.Context.Effort.Fx).SetName("N_Ed"),
                new MomentData(_input.Context.Effort.My).SetName("M_Ed,y"),
                new MomentData(_input.Context.Effort.Mz).SetName("M_Ed,z"),

                // Résistances
                new ForceData(_input.Normal.N_Rd).SetName("N_Rd"),
                new MomentData(_input.FlexionY.MRd).SetName("M_Rd,y"),
                new MomentData(_input.FlexionZ.MRd).SetName("M_Rd,z"),

                // Facteurs d’instabilité
                new CoefficientData(_input.BucklingY.Chi).SetName("χ_y"),
                new CoefficientData(_input.BucklingZ.Chi).SetName("χ_z"),
                new CoefficientData(_input.LateralBuckling.Chi).SetName("χ_LT"),

                // Paramètres Annexe A
                new CoefficientData(W_y).SetName("w_y"),
                new CoefficientData(W_z).SetName("w_z"),
                new CoefficientData(Mu_y).SetName("μ_y"),
                new CoefficientData(Mu_z).SetName("μ_z"),

                new CoefficientData(Lambda0).SetName("λ_0"),
                new CoefficientData(LambdaMin).SetName("λ_min"),
                new CoefficientData(LambdaMax).SetName("λ_max"),

                new CoefficientData(Cmy).SetName("C_my"),
                new CoefficientData(Cmz).SetName("C_mz"),

                new CoefficientData(Kyy).SetName("K_yy"),
                new CoefficientData(Kyz).SetName("K_yz"),
                new CoefficientData(Kzy).SetName("K_zy"),
                new CoefficientData(Kzz).SetName("K_zz"),
            };

            var outputs = new List<IDataType>
            {
                new CoefficientData(RatioY).SetName("Ratio_y"),
                new CoefficientData(RatioZ).SetName("Ratio_z"),
                new CoefficientData(Ratio).SetName("Ratio")
            };

            return Verification.Create(
                "Interaction N–My–Mz avec instabilités – Annexe A.3 (EC3)",
                inputs,
                outputs
            );
        }

    }
}
