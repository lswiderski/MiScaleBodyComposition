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
                DataString = "4859d53b2d3314943c58b133638c7457a4000000c3e670dc".ToLower().Replace(" ", "")
            };
        }

        [Test]
        public void Test1()
        {
            double expectedResult = 74.2;
            _inputData.DataString = "4859d53b2d3314943c58b133638c7457a4000000c3e670dc".ToLower().Replace(" ", "");
            var scale = new S400();

            var bc = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(expectedResult, bc.Weight);
        }

        [Test]
        public void TestJustMACAddress()
        {
            _inputData.DataString = "10 59 d5 3b 06 e6 a5 64 93 46 84".ToLower().Replace(" ", "");
            var scale = new S400();

            var value = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void TestNoWeight()
        {
            _inputData.DataString = "4859d53b2e724a8c783dc8a392c10db411000000a8a7bad5".ToLower().Replace(" ", "");
            var scale = new S400();

            var value = scale.GetBodyComposition(_userInfo, _inputData);

            Assert.IsTrue(true);
        }

        [Test]
        public void Test2DataStrings()
        {
            _inputData.DataString = "4859d53b2e724a8c783dc8a392c10db411000000a8a7bad5".ToLower().Replace(" ", "");
            var scale = new S400();

            var value = scale.GetBodyComposition(_userInfo, _inputData);
            _inputData.DataString = "4859d53b2d3314943c58b133638c7457a4000000c3e670dc".ToLower().Replace(" ", "");
            value = scale.GetBodyComposition(_userInfo, _inputData);
            Assert.IsTrue(true);
        }
    }
}
