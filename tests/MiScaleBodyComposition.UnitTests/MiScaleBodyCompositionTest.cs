using NUnit.Framework;

namespace MiScaleBodyComposition.UnitTests;

[TestFixture]
public class MiScaleBodyCompositionTest
{
    private byte[] _validMiScaleData;
    private byte[] _stabilizedButWithoutImpendaceData;
    private byte[] _dateData;
    private User _userInfo;

    [SetUp]
    public void Setup()
    {
        _validMiScaleData = new byte[] { 27, 24, 2, 166, 230, 7, 2, 11, 17, 34, 7, 186, 1, 60, 55 };
        _stabilizedButWithoutImpendaceData = new byte[] { 27, 24, 2, 164, 230, 7, 8, 21, 16, 1, 25, 253, 255, 104, 56 };
        _dateData = new byte[] { 27, 24, 2, 38, 230, 7, 9, 13, 14, 26, 38, 158, 1, 128, 57 };
        _userInfo = new User(182, 29, Sex.Male);
    }

    [Test]
    public void ShouldReturnProperHour()
    {
        int expectedResult = 17;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validMiScaleData, _userInfo);
        Assert.AreEqual(expectedResult, bc.Hour);
    }

    [Test]
    public void ShouldReturnProperYear()
    {
        int expectedResult = 2022;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_validMiScaleData, _userInfo);
        Assert.AreEqual(expectedResult, bc.Year);
    }

    [Test]
    public void ShouldReturnProperDate()
    {
        int year = 2022;
        int month = 9;
        int day = 13;
        int hour = 14;
        int minutes = 26;
        int seconds = 38;
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_dateData, _userInfo);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(year, bc.Year);
            Assert.AreEqual(month, bc.Month);
            Assert.AreEqual(day, bc.Day);
            Assert.AreEqual(hour, bc.Hour);
            Assert.AreEqual(minutes, bc.Minute);
            Assert.AreEqual(seconds, bc.Second);
        });

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
        var hasImpedance = scale.HasImpedance(_validMiScaleData, _userInfo);
        Assert.IsTrue(hasImpedance);
    }

    [Test]
    public void ShouldBeStabilizedButWithoutImpedance()
    {
        var scale = new MiScale();
        var isStabilized = scale.Istabilized(_stabilizedButWithoutImpendaceData, _userInfo);
        var hasNoImpedance = !scale.HasImpedance(_stabilizedButWithoutImpendaceData, _userInfo);
        Assert.IsTrue(isStabilized && hasNoImpedance);
    }

    [Test]
    public void DataWithoutImpedanceShouldBeNotNull()
    {
        var scale = new MiScale();
        var bc = scale.GetBodyComposition(_stabilizedButWithoutImpendaceData, _userInfo);
        Assert.NotNull(bc);
    }
}