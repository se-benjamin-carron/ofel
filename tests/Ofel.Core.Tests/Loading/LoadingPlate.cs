using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ofel.Core.Load;
using Ofel.Core.Load.Climatic;

namespace Ofel.Core.Tests.Loading
{
    public class LoadingPlateTests
    {
        [Fact]
        public void LoadingPlate_CalculationPermanentTests()
        {
            var point1 = new Point(0, 4, 4);
            var point2 = new Point(30, 4, 4);
            var point3 = new Point(30, 0, 0);
            var point4 = new Point(0, 0, 0);
            var allPoints = new[] { point1, point2, point3, point4 };
            // DefaultValue is < 0 because the rain direction is to the left
            var purlinsPosition = new[] { 0.2, 0.5, 0.8 };
            var gantriesPosition = new[] { 0.2, 0.5, 0.8 };
            var plate = new LoadingPlate(allPoints, purlinsPosition, gantriesPosition, 0.1);
            plate.AddPermanentLoad(10);

            Assert.True(plate.AllForces.Count == 1);
            double mesh = 0.2;
            var allPurlins = plate.GetPurlinsMember(mesh);
            var force = allPurlins[0]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected = new ForceValue(-14, 0, 16.52769985, 0, 0, 0);
            Assert.True(force.Equals(forceExpected));
            var force2 = allPurlins[1]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected2 = new ForceValue(-12, 0, 6.944600, 0, 0, 0);
            Assert.True(force2.Equals(forceExpected2));
        }

        [Fact]
        public void LoadingPlate_CalculationSnowTests()
        {
            var point1 = new Point(0, 4, 2);
            var point2 = new Point(30, 4, 2);
            var point3 = new Point(30, 0, 0);
            var point4 = new Point(0, 0, 0);
            var allPoints = new[] { point1, point2, point3, point4 };
            var angle = -14.036243;
            var purlinsPosition = new[] { 0.2, 0.5, 0.8 };
            var gantriesPosition = new[] { 0.2, 0.5, 0.8 };
            var plate = new LoadingPlate(allPoints, purlinsPosition, gantriesPosition, 0.1);
            var input = new SnowInput
            {
                altitude = 199,
                area = SnowArea.A1,
                IsProtectedFromWind = false
            };
            var snowCoef = new SnowCoefficient(Math.Abs(angle));
            SnowCharacteristics snowChar = new SnowCharacteristics(input);
            plate.AddSnowLoad1Slope(snowChar, snowCoef);
            Assert.True(plate.AllForces.Count == 2);
            double mesh = 0.2;
            var allPurlins = plate.GetPurlinsMember(mesh);
            var force = allPurlins[0]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected = new ForceValue(-251.991749, 0, 595.0788911, 0, 0, 0);
            Assert.True(force.Equals(forceExpected));
            var force2 = allPurlins[1]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected2 = new ForceValue(-215.99292783849941, 0, 249.795070043944, 0, 0, 0);
            Assert.True(force2.Equals(forceExpected2));
        }
        [Fact]
        public void LoadingPlate_CalculationWindTests()
        {
            var point1 = new Point(0, 4, 2);
            var point2 = new Point(30, 4, 2);
            var point3 = new Point(30, 0, 0);
            var point4 = new Point(0, 0, 0);
            var allPoints = new[] { point1, point2, point3, point4 };
            var angle = -14.036243;
            var purlinsPosition = new[] { 0.2, 0.5, 0.8 };
            var gantriesPosition = new[] { 0.2, 0.5, 0.8 };
            var plate = new LoadingPlate(allPoints, purlinsPosition, gantriesPosition, 0.1);
            var inputProject = new WindInputProject
            {
                area = WindArea._1,
                orographyCoefficient = 0.8,
                seasonCoefficient = 1.0,
                directionCoefficient = 1.0,
                probabilityCoefficient = 1.0,
                heightZ = 10.0
            };
            var inputQuarter = new WindInputQuarter
            {
                azimuth = 45,
                rugosity = RugosityCategory.IIIa,
            };
            WindCharacteristics windChar = new WindCharacteristics(inputProject, inputQuarter);
            WindCoefficient coef = new WindCoefficient(0.5, Math.Abs(angle));
            double[] obstructions = new[] { 0.5, 0.5, 0.5, 0.5 };
            var windChars = new WindCharacteristics[] { windChar, windChar, windChar, windChar };
            plate.AddWindLoad1SlopeCTICM(windChars, coef, 0.03, obstructions);
            Assert.True(plate.AllForces.Count == 8);
            double mesh = 0.2;
            var allPurlins = plate.GetPurlinsMember(mesh);
            var force = allPurlins[0]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected = new ForceValue(-251.991749, 0, 595.0788911, 0, 0, 0);
            Assert.True(force.Equals(forceExpected));
            var force2 = allPurlins[1]
                .Forces[0]
                .ForcesByEpsilon
                .First().Item2;
            var forceExpected2 = new ForceValue(-215.99292783849941, 0, 249.795070043944, 0, 0, 0);
            Assert.True(force2.Equals(forceExpected2));
        }

    }
}
