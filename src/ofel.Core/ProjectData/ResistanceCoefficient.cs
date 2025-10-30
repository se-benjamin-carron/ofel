namespace ofel.Core
{
    public class ProjectResistanceCoefficient
    {
       public ProjectResistanceCoefficient()
        {
            Steel = new SteelResistanceCoefficient();
            Concrete = new ConcreteResistanceCoefficient();
            Wood = new WoodResistanceCoefficient();
        }
        public SteelResistanceCoefficient Steel { get; set; }
        public ConcreteResistanceCoefficient Concrete { get; set; }
        public WoodResistanceCoefficient Wood { get; set; }
    }

    public abstract class ResistanceCoefficient
    {
    }

    public class SteelResistanceCoefficient : ResistanceCoefficient
    {
  
        public SteelResistanceCoefficient(bool isAdvanced = false, IList<double>? gammaM = null)
        {
            if (isAdvanced && gammaM != null && gammaM.Count == 6)
            {
                for (int i = 0; i < 6; i++)
                {
                    GammaM0 = gammaM[0];
                    GammaM1 = gammaM[1];
                    GammaM2 = gammaM[2];
                    GammaM3 = gammaM[3];
                    GammaM4 = gammaM[4];
                    GammaM5 = gammaM[5];
                }
            }
            else
            {
                GammaM0 = 1.0;
                GammaM1 = 1.0;
                GammaM2 = 1.25;
                GammaM3 = 1.0;
                GammaM4 = 1.0;
                GammaM5 = 1.0;
            }
        }
        public double GammaM0 { get; }
        public double GammaM1 { get; }
        public double GammaM2 { get; }
        public double GammaM3 { get; }
        public double GammaM4 { get; }
        public double GammaM5 { get; }
    }

    public class ConcreteResistanceCoefficient : ResistanceCoefficient
    {
        public double GammaC { get; }
        public double GammaCAcc { get; }

        public ConcreteResistanceCoefficient(bool isAdvanced = false, IList<double>? gammaC = null)
        {
            if (isAdvanced && gammaC != null && gammaC.Count == 2)
            {
                GammaC = gammaC[0];
                GammaCAcc = gammaC[1];
            }
            else
            {
                GammaC = 1.5;
                GammaCAcc = 1.2;
            }
        }
    }

    public class WoodResistanceCoefficient : ResistanceCoefficient
    {
        public double GammaMMassive { get; }
        public double GammaGluedLaminated { get; }
        public double GammaAssembly { get; }
        public double GammaAccidental { get; }

        public WoodResistanceCoefficient(bool isAdvanced = false, IList<double>? gamma = null)
        {
            if (isAdvanced && gamma != null && gamma.Count == 4)
            {
                GammaMMassive = gamma[0];
                GammaGluedLaminated = gamma[1];
                GammaAssembly = gamma[2];
                GammaAccidental = gamma[3];
            }
            else
            {
                GammaMMassive = 1.3;
                GammaGluedLaminated = 1.25;
                GammaAssembly = 1.3;
                GammaAccidental = 1.0;
            }
        }
    }
}