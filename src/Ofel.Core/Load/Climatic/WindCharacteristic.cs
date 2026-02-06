using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Load.Climatic
{
    public enum RugosityCategory
    {
        _0, I, II, IIIa, IIIb, IV
    }

    public enum WindArea
    {
        _1,
        _2,
        _3,
        _4
    }

    public class WindInputProject
    {
        public WindArea area;
        public double orographyCoefficient;
        public double seasonCoefficient;
        public double directionCoefficient;
        public double probabilityCoefficient;
        public double heightZ;
    }

    public class WindInputQuarter
    {
        public RugosityCategory rugosity;
        public double azimuth;
    }

    public class WindCharacteristics : IVerifiable
    {

        public double BaseWindSpeed0;
        public double DirectionCoefficient;
        public double BaseWindSpeed;
        public double RoughnessLength;
        public double RoughnessLengthMinimum;
        public double RugosityCoefficientKr;
        public double RugosityCoefficientCr;
        public double AverageWindSpeed;
        public double DynamicPressure;
        public double ExpositionFactorKl;
        public double TurbulenceCoefficientIv;
        public double ExpositionFactorCe;
        public double PeakPressure;


        public WindCharacteristics(WindInputProject inputProject, WindInputQuarter inputQuarter)
        {
            BaseWindSpeed0 = this.GetBaseWindSpeed0(inputProject.area);
            DirectionCoefficient = this.GetDirectionCoefficient(inputQuarter.azimuth);
            BaseWindSpeed = this.GetBaseWindSpeed(BaseWindSpeed0, DirectionCoefficient, inputProject.seasonCoefficient, inputProject.probabilityCoefficient);
            RoughnessLength = this.GetRoughnessLength(inputQuarter.rugosity);
            RoughnessLengthMinimum = this.GetRoughnessLengthMinimum(inputQuarter.rugosity);
            RugosityCoefficientKr = this.GetRugosityCoefficientKr(RoughnessLength);
            RugosityCoefficientCr = this.GetRugosityCoefficientCr(inputProject.heightZ, RoughnessLength, RoughnessLengthMinimum, RugosityCoefficientKr);
            AverageWindSpeed = this.GetAverageSpeedWind(RugosityCoefficientCr, inputProject.orographyCoefficient, BaseWindSpeed);
            DynamicPressure = this.GetDynamicPressure(AverageWindSpeed);
            ExpositionFactorKl = this.GetExpositionFactorKl(inputProject.orographyCoefficient, RoughnessLength);
            TurbulenceCoefficientIv = this.GetTurbulenceCoefficient(inputProject.heightZ, RoughnessLength, RoughnessLengthMinimum, ExpositionFactorKl, inputProject.orographyCoefficient);
            ExpositionFactorCe = this.GetExpositionFactorCe(TurbulenceCoefficientIv);
            PeakPressure = this.GetPeakPressure(DynamicPressure, ExpositionFactorCe);
        }

        public Verification ToVerification()
        {
            var inputs = new List<IDataType>();

            var outputs = new List<IDataType>
            {
                new SpeedData(BaseWindSpeed0).SetName("v_b0"),
                new SpeedData(BaseWindSpeed).SetName("v_b"),
                new LengthData(RoughnessLength).SetName("z_0"),
                new LengthData(RoughnessLengthMinimum).SetName("z_min"),
                new CoefficientData(RugosityCoefficientKr).SetName("k_r"),
                new CoefficientData(RugosityCoefficientCr).SetName("c_r"),
                new SpeedData(AverageWindSpeed).SetName("v_m"),
                new PressureData(DynamicPressure).SetName("q_b"),
                new CoefficientData(ExpositionFactorKl).SetName("k_l"),
                new CoefficientData(TurbulenceCoefficientIv).SetName("i_v"),
                new CoefficientData(ExpositionFactorCe).SetName("c_e"),
                new PressureData(PeakPressure).SetName("q_p")
            };

            return Verification.Create("Wind Characteristics", inputs, outputs);
        }

        public double GetBaseWindSpeed0(WindArea area)
        {
            switch (area)
            {
                case WindArea._1:
                    return 22.0;
                case WindArea._2:
                    return 24.0;
                case WindArea._3:
                    return 26.0;
                case WindArea._4:
                    return 28.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(area), "Invalid WindArea");
            }
        }

        public double GetDirectionCoefficient(double azimuth)
        {
            return 1.0;
        }

        public double GetBaseWindSpeed(double v_b0, double c_dir, double c_season, double c_prob)
        {
            return v_b0 * c_dir * c_season * c_prob;
        }

        public double GetRoughnessLength(RugosityCategory rugosity)
        {
            switch (rugosity)
            {
                case RugosityCategory._0:
                    return 0.005;
                case RugosityCategory.II:
                    return 0.05;
                case RugosityCategory.IIIa:
                    return 0.2;
                case RugosityCategory.IIIb:
                    return 0.5;
                case RugosityCategory.IV:
                    return 1.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rugosity), "Invalid RugosityCategory");
            }
        }

        public double GetRoughnessLengthMinimum(RugosityCategory rugosity)
        {
            switch (rugosity)
            {
                case RugosityCategory._0:
                    return 1.0;
                case RugosityCategory.II:
                    return 2.0;
                case RugosityCategory.IIIa:
                    return 5.0;
                case RugosityCategory.IIIb:
                    return 9.0;
                case RugosityCategory.IV:
                    return 15.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rugosity), "Invalid RugosityCategory");
            }
        }
        public double GetRugosityCoefficientKr(double z0)
        {
            double z0_2 = 0.05;
            return 0.19 * Math.Pow(z0 / z0_2, 0.07);
        }
        public double GetRugosityCoefficientCr(double z, double z0, double zmin, double kr)
        {
            return kr * Math.Log(z / Math.Min(zmin, z0));
        }
        public double GetAverageSpeedWind(double c_r, double c_0, double v_b)
        {
            return c_0 * c_r * v_b;
        }

        public double GetDynamicPressure(double v_m)
        {
            double rho = 1.225; // densité de l'air en kg/m³
            return 0.5 * rho * v_m * v_m;
        }

        public double GetExpositionFactorKl(double c_0, double z_0)
        {
            return c_0 * (1 - 0.0002 * Math.Pow(Math.Log10(z_0) + 3.0, 6));
        }
        public double GetTurbulenceCoefficient(double z, double z_0, double z_min, double k_l, double c_0)
        {
            if (z_0 < z_min)
            {
                return k_l / (c_0 * Math.Log(z / z_0));
            }
            else
            {
                return k_l / (c_0 * Math.Log(z_min / z_0));
            }
        }
        public double GetExpositionFactorCe(double i_v)
        {
            return (1 + 7 * i_v) * Math.Pow(AverageWindSpeed / BaseWindSpeed, 2);
        }

        public double GetPeakPressure(double q_b, double c_e)
        {
            return (1 + 7 * TurbulenceCoefficientIv) * 0.5 * 1.225 * Math.Pow(AverageWindSpeed, 2);
        }

    }
}
