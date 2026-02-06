using Ofel.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Load.Climatic
{

    public class SnowCoefficient : IVerifiable
    {
        public double Mu1;
        public double Mu2;
        public double Mu3;

        public SnowCoefficient(double angle)
        {
            Mu1 = this.GetMu12Coefficient(angle);
            Mu2 = this.GetMu12Coefficient(angle);
            Mu3 = this.GetMu3Coefficient(angle);
        }

        public Verification ToVerification()
        {
            var inputs = new List<IDataType>();

            var outputs = new List<IDataType>
            {
                new CoefficientData(Mu1).SetName("µ_1"),
                new CoefficientData(Mu2).SetName("µ_2"),
                new CoefficientData(Mu3).SetName("µ_3"),

            };

            return Verification.Create("Snow Coefficient", inputs, outputs);
        }
        public double GetMu12Coefficient(double angle)
        {
            if (angle >= 0 && angle < 30.0)
                return 0.8;
            else if (angle >= 30 && angle < 60)
                return 0.8 * (60.0 - angle) / 30;
            else
                return 0.6;
        }
        public double GetMu3Coefficient(double angle)
        {
            if (angle >= 0 && angle < 30.0)
                return 0.8 + 0.8 * angle / 30;
            else if (angle >= 30 && angle < 60)
                return 1.6;
            else
                throw new ArgumentOutOfRangeException(
                       nameof(angle),
                       $"Angle {angle} not implemented");
        }
    }
}
