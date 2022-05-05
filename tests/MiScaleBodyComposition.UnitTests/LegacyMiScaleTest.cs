using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiScaleBodyComposition.UnitTests
{
    [TestFixture]
    public class LegacyMiScaleTest
    {
        private byte[] _validMiScaleData;
        private double _height;

        [SetUp]
        public void Setup()
        {
            _validMiScaleData = new byte[] { 166, 60, 55, 230, 7, 2, 11, 17, 34, 7 };
            _height = 182;
        }

        [Test]
        public void ShouldReturnProperHour()
        {
            int expectedResult = 17;
            var scale = new LegacyMiScale();
            var bc = scale.GetWeight(_validMiScaleData, _height);
            Assert.AreEqual(expectedResult, bc.Hour);
        }

        [Test]
        public void ShouldReturnProperYear()
        {
            int expectedResult = 2022;
            var scale = new LegacyMiScale();
            var bc = scale.GetWeight(_validMiScaleData, _height);
            Assert.AreEqual(expectedResult, bc.Year);
        }

        [Test]
        public void ShouldReturnProperWeight()
        {
            double expectedResult = 70.7;
            var scale = new LegacyMiScale();
            var bc = scale.GetWeight(_validMiScaleData, _height);
            Assert.AreEqual(expectedResult, bc.Weight);
        }

        [Test]
        public void ShouldBeStabilized()
        {
            var scale = new LegacyMiScale();
            var isStabilized = scale.Istabilized(_validMiScaleData);
            Assert.IsTrue(isStabilized);
        }
    }
}
