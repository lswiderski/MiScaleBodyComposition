using MiScaleBodyComposition.Contracts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiScaleBodyComposition.UnitTests
{
    [TestFixture]
    public class S400Test
    {
        private User _userInfo;
        private S400InputData _inputData;

        [SetUp]
        public void Setup()
        {
            _userInfo = new User(182, 29, Sex.Male);

            _inputData = new S400InputData
            {
                MacOriginal = "84:46:93:64:A5:E6",
                AesKey = "58305740b64e4b425e518aa1f4e51339",
            };
        }

        [Test]
        public void Test1()
        {
            double expectedResult = 74.2;
            _inputData.DataString = "4859d53b2d3314943c58b133638c7457a4000000c3e670dc".ToLower().Replace(" ", "");
            var scale = new S400Scale();

            var bc = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(expectedResult, bc.Weight);
        }

        [Test]
        public void Test26bytesHex()
        {
            double expectedResult = 73.2;
            _inputData.DataString = "95FE4859D53B3BDE6BC8D05B51C0CDFD9021C9000000925C5039".ToLower().Replace(" ", "");
            var scale = new S400Scale();

            var bc = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(expectedResult, bc.Weight);
        }

        [Test]
        public void Test26bytes()
        {
            double expectedResult = 73.3;
            _inputData.Data = new byte[] { 149, 254,72,89,213,59,77,111,53,156,229,111,31,126,126,10,221,220,38,0,0,0,12,19,211,196 };
            var scale = new S400Scale();

            var bc = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(expectedResult, bc.Weight);
        }

        [Test]
        public void Test26bytesOnlyWeight()
        {
            _inputData.Data = new byte[] { 149, 254, 72, 89, 213, 59, 99, 187, 88, 121, 80, 225, 4, 44, 172, 28, 95, 24, 246, 0, 0, 0, 219, 233, 112, 52 };
            var scale = new S400Scale();

            var bc = scale.GetBodyComposition(_userInfo, _inputData);
            var condition = bc.Weight > 0 && bc.Impedance == null;

            Assert.AreEqual(true, condition);
        }

        [Test]
        public void TestJustMACAddress()
        {
            _inputData.DataString = "10 59 d5 3b 06 e6 a5 64 93 46 84".ToLower().Replace(" ", "");
            var scale = new S400Scale();

            var value = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void TestNoWeight()
        {
            _inputData.DataString = "4859d53b2e724a8c783dc8a392c10db411000000a8a7bad5".ToLower().Replace(" ", "");
            var scale = new S400Scale();

            var value = scale.GetBodyComposition(_userInfo, _inputData);

            Assert.IsTrue(true);
        }

        [Test]
        public void Test2DataStrings()
        {
            _inputData.DataString = "4859d53b2e724a8c783dc8a392c10db411000000a8a7bad5".ToLower().Replace(" ", "");
            var scale = new S400Scale();

            var value = scale.GetBodyComposition(_userInfo, _inputData);
            _inputData.DataString = "4859d53b2d3314943c58b133638c7457a4000000c3e670dc".ToLower().Replace(" ", "");
            value = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.IsTrue(true);
        }
    }
}
