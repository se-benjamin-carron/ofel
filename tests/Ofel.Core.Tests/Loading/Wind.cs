using System;
using Xunit;
using Ofel.Core.Load.Climatic;

namespace Ofel.Core.Tests.Load.Climatic
{
    public class WindCharacteristicsTests
    {
        private WindInputProject CreateDefaultProject()
        {
            return new WindInputProject
            {
                area = WindArea._2,
                orographyCoefficient = 1.0,
                seasonCoefficient = 1.0,
                probabilityCoefficient = 1.0,
                directionCoefficient = 1.0,
                heightZ = 10.0
            };
        }

        private WindInputQuarter CreateDefaultQuarter()
        {
            return new WindInputQuarter
            {
                rugosity = RugosityCategory.II,
                azimuth = 0.0
            };
        }

        #region Base wind speed

        [Theory]
        [InlineData(WindArea._1, 22.0)]
        [InlineData(WindArea._2, 24.0)]
        [InlineData(WindArea._3, 26.0)]
        [InlineData(WindArea._4, 28.0)]
        public void GetBaseWindSpeed0_ReturnsExpectedValue(WindArea area, double expected)
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            var result = wc.GetBaseWindSpeed0(area);

            Assert.Equal(expected, result);
        }

        #endregion

        #region Roughness

        [Theory]
        [InlineData(RugosityCategory._0, 0.005)]
        [InlineData(RugosityCategory.II, 0.05)]
        [InlineData(RugosityCategory.IIIa, 0.2)]
        [InlineData(RugosityCategory.IIIb, 0.5)]
        [InlineData(RugosityCategory.IV, 1.0)]
        public void GetRoughnessLength_ReturnsExpectedValue(
            RugosityCategory rugosity,
            double expected)
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            var result = wc.GetRoughnessLength(rugosity);

            Assert.Equal(expected, result, 6);
        }

        [Theory]
        [InlineData(RugosityCategory._0, 1.0)]
        [InlineData(RugosityCategory.II, 2.0)]
        [InlineData(RugosityCategory.IIIa, 5.0)]
        [InlineData(RugosityCategory.IIIb, 9.0)]
        [InlineData(RugosityCategory.IV, 15.0)]
        public void GetRoughnessLengthMinimum_ReturnsExpectedValue(
            RugosityCategory rugosity,
            double expected)
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            var result = wc.GetRoughnessLengthMinimum(rugosity);

            Assert.Equal(expected, result);
        }

        #endregion

        #region Coefficients

        [Fact]
        public void GetRugosityCoefficientKr_ReturnsPositiveValue()
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            var kr = wc.GetRugosityCoefficientKr(0.05);

            Assert.True(kr > 0);
        }

        [Fact]
        public void GetDynamicPressure_IsCorrect()
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            double v = 10.0;
            double expected = 0.5 * 1.225 * v * v;

            var result = wc.GetDynamicPressure(v);

            Assert.Equal(expected, result, 6);
        }

        [Fact]
        public void GetPeakPressure_IsProduct()
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            double qb = 500.0;
            double ce = 2.5;

            var result = wc.GetPeakPressure(qb, ce);

            Assert.Equal(827.6158801357, result, 1e-5);
        }

        #endregion

        #region Constructor integration

        [Fact]
        public void Constructor_ComputesAllFields()
        {
            var project = CreateDefaultProject();
            var quarter = CreateDefaultQuarter();

            var wc = new WindCharacteristics(project, quarter);

            Assert.True(wc.BaseWindSpeed0 == 24.0);
            Assert.True(wc.BaseWindSpeed == 24.0);
            Assert.True(wc.RoughnessLength == 0.05);
            Assert.True(wc.AverageWindSpeed == 24.160327191459047);
            Assert.True(wc.DynamicPressure == 357.52936362399271);
            Assert.True(wc.PeakPressure == 827.61588013575238);
        }

        #endregion

        #region Exceptions

        [Fact]
        public void GetBaseWindSpeed0_InvalidArea_Throws()
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                wc.GetBaseWindSpeed0((WindArea)99));
        }

        [Fact]
        public void GetRoughnessLength_InvalidCategory_Throws()
        {
            var wc = new WindCharacteristics(CreateDefaultProject(), CreateDefaultQuarter());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                wc.GetRoughnessLength((RugosityCategory)99));
        }

        #endregion
    }
}
