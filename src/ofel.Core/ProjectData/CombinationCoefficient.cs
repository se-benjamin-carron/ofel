namespace ofel.Core
{
    public class CombinationCoefficient
    {
        public double[] Permanent { get; set; }
        public double[] Variable { get; set; }

        public CombinationCoefficient(double[] permanent, double[] variable)
        {
            Permanent = permanent;
            Variable = variable;
        }
    }

    public class CaseCoefficient
    {
        public double psi0 { get; set; }

        public double psi1 { get; set; }
        public double psi2 { get; set; }


        public CaseCoefficient(double psi0, double psi1, double psi2)
        {
            this.psi0 = psi0;
            this.psi1 = psi1;
            this.psi2 = psi2;
        }
    }

    public class WindCaseCoefficient : CaseCoefficient
    {
        public WindCaseCoefficient()
            : base(0.6, 0.2, 0.0)
        {
        }
    }
    public class SnowCaseCoefficient : CaseCoefficient
    {
        public SnowCaseCoefficient(double altitude)
            : base(altitude < 1000 ? 0.5 : 0.7, altitude < 1000 ? 0.2 : 0.5, altitude < 1000 ? 0.0 : 0.2)
        {
        }
    }
    public class ExploitationCaseCoefficient : CaseCoefficient
    {
        public ExploitationCaseCoefficient(BuildingUse use)
            : base(
                use switch
                {
                    BuildingUse.Residential => 0.7,
                    BuildingUse.Office => 0.7,
                    BuildingUse.MeetingRoom => 0.7,
                    BuildingUse.Shop => 0.7,
                    BuildingUse.Storage => 1.0,
                    BuildingUse.LightTraffic => 0.7,
                    BuildingUse.HeavyTraffic => 0.7,
                    BuildingUse.RoofOnly => 0.0,
                    _ => 0.5
                },
                use switch
                {
                    BuildingUse.Residential => 0.5,
                    BuildingUse.Office => 0.5,
                    BuildingUse.MeetingRoom => 0.7,
                    BuildingUse.Shop => 0.7,
                    BuildingUse.Storage => 0.9,
                    BuildingUse.LightTraffic => 0.7,
                    BuildingUse.HeavyTraffic => 0.5,
                    BuildingUse.RoofOnly => 0.0,
                    _ => 0.5
                },
                use switch
                {
                    BuildingUse.Residential => 0.3,
                    BuildingUse.Office => 0.3,
                    BuildingUse.MeetingRoom => 0.6,
                    BuildingUse.Shop => 0.6,
                    BuildingUse.Storage => 0.8,
                    BuildingUse.LightTraffic => 0.6,
                    BuildingUse.HeavyTraffic => 0.3,
                    BuildingUse.RoofOnly => 0.0,
                    _ => 0.5
                })
        {
        }
    }
    public class TemperatureCaseCoefficient : CaseCoefficient
    {
        public TemperatureCaseCoefficient()
        : base(0.6, 0.5, 0.0)
        {
        }
    }
    public class AccidentalCaseCoefficient : CaseCoefficient
    {
        public AccidentalCaseCoefficient()
        : base(1.0, 0.0, 0.0)
        {
        }
    }
}
