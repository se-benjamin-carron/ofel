using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{


    public class LimitClass
    {
        public double Class2 { get; set; } = -1;
        public double Class3 { get; set; } = -1;
        public double Class4 { get; set; } = -1;

        public LimitClass(double class2, double class3, double class4)
        {
            Class2 = class2;
            Class3 = class3;
            Class4 = class4;
        }

        public int GetClass(double slenderness)
        {
            if (slenderness <= Class2)
            {
                return 1;
            }
            else if (slenderness <= Class3)
            {
                return 2;
            }
            else if (slenderness <= Class4)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }
    }

    public abstract class ClassSection : IVerifiable
    {
        public abstract double NEd { get; }
        public abstract double Epsilon { get; }
        public abstract double Psi { get; }
        public abstract int DesignClass { get; }

        public abstract Verification ToVerification();

        public double GetPsi(double N_ed, double area, double f_y)
        {
            return 2 * N_ed / (area * f_y) - 1.0;
        }
    }

    public sealed class ClassSectionInput
    {
        public SteelSection Section { get; }
        public SteelMaterial Material { get; }
        public double NEd { get; }

        public ClassSectionInput(
            SteelSection section,
            SteelMaterial material,
            double nEd)
        {
            Section = section;
            Material = material;
            NEd = nEd;
        }
    }

    public static class ClassSectionFactory
    {
        public static ClassSection Create(ClassSectionInput Input)
        {
            return Input.Section.ProfileType switch
            {
                "HEA" or "IPE" => new ClassSection_IPEHEA(Input),

                "TCAR" => new ClassSection_Tube(Input),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
