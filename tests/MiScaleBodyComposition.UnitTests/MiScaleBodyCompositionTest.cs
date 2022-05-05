using NUnit.Framework;

namespace MiScaleBodyComposition.UnitTests;

[TestFixture]
public class MiScaleBodyCompositionTest
{
    private byte[] _validMiScaleData;
    private User _userInfo;

    [SetUp]
    public void Setup()
    {
     _validMiScaleData = new byte[] {27,24,2,166,230,7,2,11,17,34,7,186,1,60,55 };
     _userInfo = new User(182, 29, Sex.Male);
    }

    [Test]
    public void ShouldReturnProperHour()
    {
        int expectedResult = 17;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validMiScaleData, _userInfo);
        Assert.AreEqual(expectedResult,bc.Hour);
    }

    [Test]
    public void ShouldReturnProperYear()
    {
        int expectedResult = 2022;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validMiScaleData, _userInfo);
        Assert.AreEqual(expectedResult,bc.Year);
    }

     [Test]
    public void ShouldReturnProperWeight()
    {
        double expectedResult = 70.7;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validMiScaleData, _userInfo);
        Assert.AreEqual(expectedResult, bc.Weight);
    }

    [Test]
    public void ShouldBeStabilized()
    {
        var scale = new MiScale();
        var isStabilized = scale.Istabilized(_validMiScaleData, _userInfo);
        Assert.IsTrue(isStabilized);
    }

    [Test]
    public void ShouldHaveImpedance()
    {
        var scale = new MiScale();
        var isStabilized = scale.HasImpedance(_validMiScaleData, _userInfo);
        Assert.IsTrue(isStabilized);
    }
}