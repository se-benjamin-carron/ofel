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
    public sealed class ClassSection_Tube : ClassSection
    {
        public override double NEd { get; }
        public override double Epsilon { get; }
        public double LengthBase { get; }
        public double LengthHeight { get; }
        public double SlendernessBase { get; }
        public double SlendernessHeight { get; }
        public double AlphaHeight { get; }
        public double AlphaBase { get; }

        public override double Psi { get; }
        public LimitClass LimitClassHeight { get; }
        public LimitClass LimitClassBase { get; }
        public int HeightClass { get; }
        public int BaseClass { get; }

        public override int DesignClass { get; }

        public ClassSection_Tube(ClassSectionInput Input)
        {
            double EffortN = Input.NEd;
            SteelSection section = Input.Section;
            SteelMaterial material = Input.Material;
            NEd = EffortN;
            Epsilon = Input.Material.GetEpsilon();
            LengthHeight = GetLengthH(section);
            LengthBase = GetLengthB(section);
            SlendernessBase = LengthBase / section.T_f;
            SlendernessHeight = LengthHeight / section.T_w;
            AlphaHeight = GetAlpha(LengthHeight, section.T_w, material.Fy, NEd);
            AlphaBase = GetAlpha(LengthBase, section.T_f, material.Fy, NEd);

            Psi = GetPsi(NEd, section.A, material.Fy);
            LimitClassHeight = GetLimitClassEdge(AlphaHeight, Psi, Epsilon);
            LimitClassBase = GetLimitClassEdge(AlphaBase, Psi, Epsilon);
            HeightClass = LimitClassHeight.GetClass(SlendernessHeight);
            BaseClass = LimitClassBase.GetClass(SlendernessBase);

            DesignClass = Math.Max(BaseClass, HeightClass);
        }
        public double GetAlpha(double c, double t_w, double f_y, double N_ed)
        {
            if (N_ed >= 2 * c * t_w * f_y)
            {
                return 1.0;
            }
            else if (N_ed <= -2 * c * t_w * f_y)
            {
                return 0.0;
            }
            else
            {
                return 0.5 * (1.0 + N_ed / (2 * c * t_w * f_y));
            }
        }
        public double GetLengthH(SteelSection section)
        {
            return section.H - 2 * (section.R_1 + section.T_f);
        }

        public double GetLengthB(SteelSection section)
        {
            return section.B - 2 * (section.R_1 + section.T_w);
        }

        public override Verification ToVerification()
        {
            var inputs = new List<IDataType>
                {
                    new ForceData(NEd).SetName("N_Ed"),
                    new LengthData(SlendernessBase).SetName("Elancement base"),
                    new LengthData(SlendernessHeight).SetName("Elancement hauteur"),

                };
            var outputs = new List<IDataType>
                {
                    new CoefficientData(Epsilon).SetName("e"),
                    new CoefficientData(Psi).SetName("psi"),

                    new CoefficientData(AlphaBase).SetName("a_b"),
                    new CoefficientData(LimitClassBase.Class2).SetName("Limite Classe 2"),
                    new CoefficientData(LimitClassBase.Class3).SetName("Limite Classe 3"),
                    new CoefficientData(LimitClassBase.Class4).SetName("Limite Classe 4"),
                    new CoefficientData(BaseClass).SetName("Classe de la largeur"),

                    new CoefficientData(AlphaHeight).SetName("a_h"),
                    new CoefficientData(LimitClassHeight.Class2).SetName("Limite Classe 2"),
                    new CoefficientData(LimitClassHeight.Class3).SetName("Limite Classe 3"),
                    new CoefficientData(LimitClassHeight.Class4).SetName("Limite Classe 4"),
                    new CoefficientData(HeightClass).SetName("Classe de la hauteur"),

                    new CoefficientData(DesignClass).SetName("Classe de Section"),
                };
            return Verification.Create("Classe de la section Tube", inputs, outputs);
        }

        public LimitClass GetLimitClassEdge(double alpha, double psi, double epsilon)
        {
            double class2 = 0;
            double class3 = 0;
            double class4 = 0;

            if (alpha > 0.5)
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

    }
}
