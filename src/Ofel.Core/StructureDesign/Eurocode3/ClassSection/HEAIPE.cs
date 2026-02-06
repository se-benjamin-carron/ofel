using Microsoft.EntityFrameworkCore.Storage.Json;
using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class ClassSection_IPEHEA : ClassSection
    {
        public override double NEd { get; }
        public override double Epsilon { get; }
        public double Alpha { get; }
        public override double Psi { get; }
        public double HeightWeb { get; }
        public double HeightFlange { get; }
        public double SlendernessWeb { get; }
        public double SlendernessFlange { get; }
        public LimitClass? LimitClassWeb { get; }
        public LimitClass? LimitClassFlange { get; }
        public int FlangeClass { get; }
        public int WebClass { get; }
        public override int DesignClass { get; }

        public ClassSection_IPEHEA(ClassSectionInput inputIPEHEA)
        {
            double EffortN = inputIPEHEA.NEd;
            SteelSection section = inputIPEHEA.Section;
            SteelMaterial Material = inputIPEHEA.Material;
            NEd = EffortN;
            // If NEd >= 0 -> alpha 1 Compressed and Ned<= 0 Traction -> 0 Beware
            Epsilon = inputIPEHEA.Material.GetEpsilon();
            HeightWeb = this.GetHeightWeb(section);
            HeightFlange = this.GetHeightFlange(section);
            SlendernessWeb = GetSlendernessWeb(section);
            SlendernessFlange = GetSlendernessFlange(section);
            Alpha = GetAlpha(HeightWeb, section.T_w, Material.Fy, NEd);
            Psi = GetPsi(EffortN, section.A, Material.Fy);
            LimitClassWeb = GetLimitClassWeb(Alpha, Psi, Epsilon);
            LimitClassFlange = GetLimitClassFlange(Epsilon);
            WebClass = LimitClassWeb.GetClass(SlendernessWeb);
            FlangeClass = LimitClassFlange.GetClass(SlendernessFlange);
            DesignClass = Math.Max(WebClass, FlangeClass);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(NEd).SetName("N_Ed"),
                };
            var outputs = new List<IDataType>
                {
                    new CoefficientData(Epsilon).SetName("e"),
                    new CoefficientData(Alpha).SetName("a"),
                    new CoefficientData(Psi).SetName("psi"),
                    new LengthData(SlendernessWeb).SetName("c_w"),
                    new CoefficientData(LimitClassWeb.Class2).SetName("Limite Classe 2-Ame"),
                    new CoefficientData(LimitClassWeb.Class3).SetName("Limite Classe 3-Ame"),
                    new CoefficientData(LimitClassWeb.Class4).SetName("Limite Classe 4-Ame"),
                    new CoefficientData(WebClass).SetName("Classe de l'âme"),
                    new LengthData(SlendernessFlange).SetName("c_f"),
                    new CoefficientData(LimitClassFlange.Class2).SetName("Limite Classe 2-Semelles"),
                    new CoefficientData(LimitClassFlange.Class3).SetName("Limite Classe 3-Semelles"),
                    new CoefficientData(LimitClassFlange.Class4).SetName("Limite Classe 4-Semelles"),
                    new CoefficientData(FlangeClass).SetName("Classe des semelles"),
                    new CoefficientData(DesignClass).SetName("Classe de Section"),
                };
            return Verification.Create("Classe de la section Tube", inputs, outputs);
        }
        public double GetAlpha(double c, double t_w, double f_y, double N_ed)
        {
            if (N_ed >= c * t_w * f_y)
            {
                return 1.0;
            }
            else if (N_ed <= -c * t_w * f_y)
            {
                return 0.0;
            }
            else
            {
                return 0.5 * (1.0 + N_ed / (c * t_w * f_y));
            }
        }
        public double GetHeightWeb(SteelSection section)
        {
            return section.H - 2 * (section.R_1 + section.T_f);
        }
        public double GetHeightFlange(SteelSection section)
        {
            return 0.5 * (section.B - 2 * section.R_1 - section.T_w);
        }

        public double GetSlendernessWeb(SteelSection section)
        {
            return HeightWeb / section.T_w;
        }

        public double GetSlendernessFlange(SteelSection section)
        {
            return HeightFlange / section.T_f;
        }

        public LimitClass GetLimitClassWeb(double alpha, double psi, double epsilon)
        {
            double class2 = 0;
            double class3 = 0;
            double class4 = 0;

            if (alpha >= 0.5)
            {
                class2 = 396 * epsilon / (13 * alpha - 1);
                class3 = 456 * epsilon / (13 * alpha - 1);
            }
            else
            {
                class2 = 36 * epsilon / alpha;
                class3 = 41 * epsilon / alpha;
            }
            if (psi >= -1)
            {
                class4 = 42 * epsilon / (0.67 + 0.33 * psi);
            }
            else
            {
                class4 = 62 * epsilon * (1 - psi) * Math.Pow(-psi, 0.5);
            }
            return new LimitClass(class2, class3, class4);
        }

        public LimitClass GetLimitClassFlange(double epsilon)
        {
            double class2 = 9 * epsilon;
            double class3 = 10 * epsilon;
            double class4 = 14 * epsilon;
            return new LimitClass(class2, class3, class4);
        }
    }
}
