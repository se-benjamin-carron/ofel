namespace Ofel.Core
{

    public enum BuildingUse
    {
        Residential,
        Office,
        MeetingRoom,
        Shop,
        Storage,
        LightTraffic,
        HeavyTraffic,
        RoofOnly
    }

    public class ProjectLoadCoefficient
    {
        public CaseCoefficient Exploitation { get; set; }
        public CaseCoefficient Wind { get; set; }
        public CaseCoefficient Snow { get; set; }
        public CaseCoefficient Temperature { get; set; }
        public CaseCoefficient Accidental { get; set; }
        public CombinationCoefficient Uls { get; set; }
        public CombinationCoefficient UlsAcc { get; set; }
        public CombinationCoefficient Sls { get; set; }


        public ProjectLoadCoefficient(BuildingUse use, double altitude)
        {
            Exploitation = new ExploitationCaseCoefficient(use);
            Wind = new WindCaseCoefficient();
            Snow = new SnowCaseCoefficient(altitude);
            Temperature = new TemperatureCaseCoefficient();
            Accidental = new CaseCoefficient(0.8, 0.5, 0.0);
            Uls = new CombinationCoefficient(
                new double[] { 1.35, 1.0, },
                new double[] { 1.5 });
            UlsAcc = new CombinationCoefficient(
                new double[] { 1.0, },
                new double[] { 1.0 });
            Sls = new CombinationCoefficient(
                new double[] { 1.0 },
                new double[] { 1.0 });
        }
    }
}
