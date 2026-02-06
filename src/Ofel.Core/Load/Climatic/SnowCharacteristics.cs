using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Load.Climatic
{
    public enum SnowArea
    {
        A1,
        A2,
        B1,
        B2,
        C1,
        C2,
        D,
        E
    }

    public class SnowInput
    {
        public SnowArea area;
        public double altitude;
        public bool IsProtectedFromWind;
    }

    public enum AddedSnowType
    {
        Delta_s1,
        Delta_s2
    }
    public class SnowCharacteristics : IVerifiable
    {
        public double ExpositionCoefficient;
        public double CharacteristicSnow;
        public double AccidentelSnow;
        public double AltitudeAddedSnow;
        public AddedSnowType AltitudeAddedSnowRegion;

        public SnowCharacteristics(SnowInput input)
        {
            ExpositionCoefficient = this.getExposureCoefficient(input.IsProtectedFromWind);
            CharacteristicSnow = this.getCharacteristicSnow(input.area);
            AccidentelSnow = this.getAccidentelSnow(input.area);
            AltitudeAddedSnowRegion = this.getTypeAddedSnow(input.area);
            AltitudeAddedSnow = this.delta_s(AltitudeAddedSnowRegion, input.altitude);
        }
        public Verification ToVerification()
        {
            var inputs = new List<IDataType>();
            var outputs = new List<IDataType>();
            CoefficientData ce = new CoefficientData(ExpositionCoefficient).SetName("c_e");
            PressureData sk = new PressureData(CharacteristicSnow).SetName("s_k");
            PressureData sacc = new PressureData(AccidentelSnow).SetName("s_acc");
            PressureData delta_s = new PressureData(AltitudeAddedSnow).SetName(AltitudeAddedSnowRegion.ToString());
            outputs.Add(ce);
            outputs.Add(sk);
            outputs.Add(sacc);
            outputs.Add(delta_s);

            return Verification.Create("Snow Characteristics", inputs, outputs);
        }

        public double getExposureCoefficient(bool isProtected)
        {
            return isProtected ? 1.25 : 1.0;
        }

        public double getCharacteristicSnow(SnowArea area)
        {
            switch (area)
            {
                case SnowArea.A1:
                    return 450.0;
                case SnowArea.A2:
                    return 450.0;
                case SnowArea.B1:
                    return 550.0;
                case SnowArea.B2:
                    return 550.0;
                case SnowArea.C1:
                    return 650.0;
                case SnowArea.C2:
                    return 650.0;
                case SnowArea.D:
                    return 900.0;
                case SnowArea.E:
                    return 1400.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(area), "Invalid Snow Area");
            }
        }

        public double getAccidentelSnow(SnowArea area)
        {
            switch (area)
            {
                case SnowArea.A1:
                    return 0.0;
                case SnowArea.A2:
                    return 1000.0;
                case SnowArea.B1:
                    return 0.0;
                case SnowArea.B2:
                    return 1350.0;
                case SnowArea.C1:
                    return 0.0;
                case SnowArea.C2:
                    return 1350.0;
                case SnowArea.D:
                    return 1350.0;
                case SnowArea.E:
                    return 0.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(area), "Invalid Snow Area");
            }
        }

        public AddedSnowType getTypeAddedSnow(SnowArea area)
        {
            if (area == SnowArea.E)
            {
                return AddedSnowType.Delta_s2;
            }
            else
            {
                return AddedSnowType.Delta_s1;
            }
        }

        public double delta_s(AddedSnowType name_zone, double altitude)
        {
            if (name_zone == AddedSnowType.Delta_s2)
            {
                return delta_s2(altitude);
            }
            else
            {
                return delta_s1(altitude);
            }
        }

        public double delta_s1(double altitude)
        {
            if (altitude <= 200.0)
            {
                return 0.0;
            }
            else if (altitude > 200.0 && altitude < 500.0)
            {
                return altitude - 200;
            }
            else if (altitude >= 500.0 && altitude <= 1000.0)
            {
                return 1.5 * altitude - 450;
            }
            else if (altitude > 1000.0 && altitude < 2000.0)
            {
                return 3.5 * altitude - 2450;
            }
            else
            {
                return 1350.0;
            }
        }
        public double delta_s2(double altitude)
        {
            if (altitude <= 200.0)
            {
                return 0.0;
            }
            else if (altitude > 200.0 && altitude < 500.0)
            {
                return 1.5 * altitude - 300;
            }
            else if (altitude >= 500.0 && altitude <= 1000.0)
            {
                return 3.5 * altitude - 1300;
            }
            else if (altitude > 1000.0 && altitude < 2000.0)
            {
                return 7 * altitude - 4800;
            }
            else
            {
                return 1350.0;
            }
        }
    }
}
