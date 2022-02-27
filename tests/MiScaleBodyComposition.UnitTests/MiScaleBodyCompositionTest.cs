using NUnit.Framework;

namespace MiScaleBodyComposition.UnitTests;

[TestFixture]
public class MiScaleBodyCompositionTest
{
    private byte[] _validData;
    private User _userInfo;

    [SetUp]
    public void Setup()
    {
     _validData = new byte[] {27,24,2,166,230,7,2,11,17,34,7,186,1,60,55 };
     _userInfo = new User(182, 29, Sex.Male);
    }

    [Test]
    public void ShouldReturnProperHour()
    {
        int expectedResult = 17;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validData, _userInfo);
        Assert.AreEqual(expectedResult,bc.Hour);
    }

    [Test]
    public void ShouldReturnProperYear()
    {
        int expectedResult = 2022;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validData, _userInfo);
        Assert.AreEqual(expectedResult,bc.Year);
    }
}