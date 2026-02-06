using Ofel.Core.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Load.Climatic
{
    public class WindCoefficientCTICM : IVerifiable
    {
        public double CoefficientF;
        public double Coefficient1;
        public double Coefficient2;
        public double Coefficient3;
        public WindCoefficientCTICM(double c_fr, double c1, double c2, double c3)
        {
            CoefficientF = c_fr;
            Coefficient1 = c1;
            Coefficient2 = c2;
            Coefficient3 = c3;
        }

        public Verification ToVerification()
        {
            var inputs = new List<IDataType>();

            var outputs = new List<IDataType>
            {
                new CoefficientData(CoefficientF).SetName("c_fr"),
                new CoefficientData(Coefficient1).SetName("c_2"),
                new CoefficientData(Coefficient2).SetName("c_2"),
                new CoefficientData(Coefficient3).SetName("c_3"),

            };

            return Verification.Create("Snow Coefficient", inputs, outputs);
        }
    }

    public class WindCoefficient : IVerifiable
    {
        public WindCoefficientCTICM CoefficientLifting;
        public WindCoefficientCTICM CoefficientCollasping;
        public WindCoefficientCTICM CoefficientFrictionLifting;
        public WindCoefficientCTICM CoefficientFrictionCollapsing;

        public WindCoefficient(double coefficientObstruction, double angle_value)
        {

            int[] bounds = this.Get_bound_angle(angle_value);
            double[][] coefs_lower = this.GetCoefficientsBound(bounds[0]);
            double[][] coefs_upper = this.GetCoefficientsBound(bounds[1]);
            double[][] coef_interp_angle = this.GetInterpolation2Angle(coefs_lower, coefs_upper, angle_value, bounds[0]);
            double[][] coef_interp_obstruction = this.GetInterpolationObstruction(coef_interp_angle, coefficientObstruction);
            double[][] coef_friction = this.GetCoefficientsBound(0);
            double[][] coef_friction_interpolated = this.GetInterpolationObstruction(coef_friction, coefficientObstruction);

            CoefficientLifting = new WindCoefficientCTICM(
                coef_interp_obstruction[0][0],
                coef_interp_obstruction[1][0],
                coef_interp_obstruction[2][0],
                coef_interp_obstruction[3][0]
            );
            CoefficientCollasping = new WindCoefficientCTICM(
                coef_interp_obstruction[0][1],
                coef_interp_obstruction[1][1],
                coef_interp_obstruction[2][1],
                coef_interp_obstruction[3][1]
            );
            CoefficientFrictionLifting = new WindCoefficientCTICM(
                coef_friction_interpolated[0][0],
                coef_friction_interpolated[1][0],
                coef_friction_interpolated[2][0],
                coef_friction_interpolated[3][0]
            );
            CoefficientFrictionCollapsing = new WindCoefficientCTICM(
                coef_friction_interpolated[0][1],
                coef_friction_interpolated[1][1],
                coef_friction_interpolated[2][1],
                coef_friction_interpolated[3][1]
            );
        }
        public Verification ToVerification()
        {
            var inputs = new List<IDataType>();

            var outputs = new List<IDataType>
            {
                new DataVerification(CoefficientLifting.ToVerification()),
                new DataVerification(CoefficientCollasping.ToVerification()),
                new DataVerification(CoefficientFrictionLifting.ToVerification()),
                new DataVerification(CoefficientFrictionCollapsing.ToVerification()),
            };

            return Verification.Create("Wind Characteristics", inputs, outputs);
        }


        public int[] Get_bound_angle(double angle)
        {
            switch (angle)
            {
                case >= 0 and < 5:
                    return new int[] { 0, 5 };
                case >= 5 and < 10:
                    return new int[] { 5, 10 };
                case >= 10 and < 15:
                    return new int[] { 10, 15 };
                case >= 15 and < 20:
                    return new int[] { 15, 20 };
                case >= 20 and < 25:
                    return new int[] { 20, 25 };
                case >= 25 and <= 30:
                    return new int[] { 25, 30 };
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(angle),
                        $"Angle {angle} not implemented");
            }
        }

        public double[][] GetCoefficientsBound(int angle)
        {
            switch (angle)
            {
                case 0:
                    return new[]
                    {
                new double[] { 0.2, -0.5, -1.3 },
                new double[] { 1.1, -1.4, -2.95 },
                new double[] { 0.13, -1.0, -2.92 },
                new double[] { 0.06, 0.19, 0.67 }
            };

                case 5:
                    return new[]
                    {
                new double[] { 0.4, -0.7, -1.4 },
                new double[] { 1.3, -1.8, -3.15 },
                new double[] { 0.7, -1.47, -3.14 },
                new double[] { -0.11, 0.31, 0.73 }
            };

                case 10:
                    return new[]
                    {
                new double[] { 0.5, -0.9, -1.4 },
                new double[] { 1.6, -2.1, -3.15 },
                new double[] { 0.89, -1.99, -3.14 },
                new double[] { -0.15, 0.45, 0.73 }
            };

                case 15:
                    return new[]
                    {
                new double[] { 0.7, -1.1, -1.4 },
                new double[] { 1.8, -2.5, -3.0 },
                new double[] { 1.08, -1.85, -2.44 },
                new double[] { 0.08, -0.04, 0.0 }
            };

                case 20:
                    return new[]
                    {
                new double[] { 0.8, -1.3, -1.4 },
                new double[] { 2.1, -2.9, -3.0 },
                new double[] { 0.76, -1.5, -1.67 },
                new double[] { 0.54, -0.76, -0.78 }
            };

                case 25:
                    return new[]
                    {
                new double[] { 1.0, -1.6, -1.4 },
                new double[] { 2.3, -3.2, -2.8 },
                new double[] { 1.11, -2.02, -1.77 },
                new double[] { 0.59, -0.83, -0.73 }
            };

                case 30:
                    return new[]
                    {
                new double[] { 1.2, -1.8, -1.4 },
                new double[] { 2.4, -3.6, -2.7 },
                new double[] { 1.51, -2.27, -1.82 },
                new double[] { 0.62, -0.94, -0.7 }
            };

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(angle),
                        $"Angle {angle} not implemented");

            }
        }
        public double[][] GetInterpolation2Angle(
            double[][] coefs_low,
            double[][] coefs_up,
            double angle,
            double angle_low_bound)
        {
            double[][] coef_calc = new double[coefs_low.Length][];

            for (int i = 0; i < coefs_low.Length; i++)
            {
                coef_calc[i] = new double[coefs_low[i].Length];

                for (int j = 0; j < coefs_low[i].Length; j++)
                {
                    double coef_low = coefs_low[i][j];
                    double coef_up = coefs_up[i][j];

                    coef_calc[i][j] =
                        coef_low + (coef_up - coef_low) * (angle - angle_low_bound);
                }
            }

            return coef_calc;
        }

        public double[][] GetInterpolationObstruction(double[][] all_coefs, double obstruction)
        {
            double[][] coef_interpolated = new double[4][];

            for (int i = 0; i < 4; i++)
            {
                coef_interpolated[i] = new double[2];

                coef_interpolated[i][0] = all_coefs[i][0];
                coef_interpolated[i][1] =
                    all_coefs[i][1] + (all_coefs[i][2] - all_coefs[i][1]) * obstruction;
            }

            return coef_interpolated;
        }
    }
}
