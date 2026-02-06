using Ofel.Core.Data;
using Ofel.Core.SectionParameter;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public sealed class SteelFlexionResult_Class12_Y : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class12_Y(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Y;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_pl_y;
        public override double MEd => _input.Context.Effort.My;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 1/2 – Axe Y (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_Ed,Y"),
                new AreaData(W).SetName("W_pl,y"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_Rd,1,Y"),
                new CoefficientData(Ratio).SetName("η")
                }
            );
        }
    }

    public sealed class SteelFlexionResult_Class12_Z : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class12_Z(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Z;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_pl_z;
        public override double MEd => _input.Context.Effort.Mz;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 1/2 – Axe Z (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_z, Ed"),
                new AreaData(W).SetName("W_pl,z"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_Rd,1,z"),
                new CoefficientData(Ratio).SetName("Ratio")
                }
            );
        }
    }

    public sealed class SteelFlexionResult_Class3_Y : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class3_Y(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Y;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_el_y;
        public override double MEd => _input.Context.Effort.My;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 3 – Axe Y (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_Ed,Y"),
                new AreaData(W).SetName("W_El,y"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_EL8Rd,1,Y"),
                new CoefficientData(Ratio).SetName("η")
                }
            );
        }
    }

    public sealed class SteelFlexionResult_Class3_Z : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class3_Z(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Z;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_el_z;
        public override double MEd => _input.Context.Effort.Mz;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 3 – Axe Z (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_Ed,Z"),
                new AreaData(W).SetName("W_El,z"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_EL_Rd,2"),
                new CoefficientData(Ratio).SetName("Ratio")
                }
            );
        }
    }

    public sealed class SteelFlexionResult_Class4_Y : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class4_Y(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Y;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_el_y;
        public override double MEd => _input.Context.Effort.My;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 4 – Axe Y (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_Ed,y"),
                new AreaData(W).SetName("W_El,y"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_eff_Rd"),
                new CoefficientData(Ratio).SetName("Ratio")
                }
            );
        }
    }

    public sealed class SteelFlexionResult_Class4_Z : SteelFlexion
    {
        private readonly SteelFlexionInput _input;

        public SteelFlexionResult_Class4_Z(SteelFlexionInput input)
        {
            _input = input;
        }
        public override Axis Axis => Axis.Z;
        public override int SectionClass => _input.Context.SectionClass.DesignClass;
        public override double ReductionCoef => _input.ReductionCoef;
        public override double W => _input.Context.Section.W_el_z;
        public override double MEd => _input.Context.Effort.Mz;
        public override double Fy => _input.Context.Material.Fy;
        public override double GammaM0 => _input.Context.Coef.GammaM0;


        public override Verification ToVerification()
        {
            return Verification.Create(
                "Flexion ELU – Classe 4 – Axe Z (EC3)",
                new IDataType[]
                {
                new ForceData(MEd).SetName("M_Ed,Z"),
                new AreaData(W).SetName("W_Eff,z"),
                new ResistanceData(Fy).SetName("f_y"),
                },
                new IDataType[]
                {
                new ForceData(MRd).SetName("M_Eff_Rd"),
                new CoefficientData(Ratio).SetName("Ratio")
                }
            );
        }
    }
}
