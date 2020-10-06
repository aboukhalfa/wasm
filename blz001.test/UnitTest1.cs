using NUnit.Framework;
using Services;
using System;
using System.Collections.Generic;

namespace blz001.test
{
  public class Tests
  {
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MidnightGivesZero()
    {
      var sut = new TimeArranger();
      var list = new List<DateTime>();
      var result = sut.GetArrangement(list, DateTime.Now.Date);

      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void MidnightToFirstTime()
    {
      var sut = new TimeArranger();
      var list = new List<DateTime>();
      var result = sut.GetArrangement(list, DateTime.Now.Date);

      Assert.That(result, Is.EqualTo(0));
    }

    [TestCaseSource(typeof(TimesDataClass), "TestCases")]
    public double DivideTest(IEnumerable<DateTime> times, DateTime dt)
    {
      var sut = new TimeArranger();
      var result = sut.GetArrangement(times, dt);
      return result;
    }
  }

  public class TimesDataClass
  {
    public static IEnumerable<TestCaseData> TestCases
    {
      get
      {
        var midnight = DateTime.Now.Date;
        var firstTime4AM = midnight.AddHours(4);
        var secondTime12PM =midnight.AddHours(12);
        var thirdTime15PM =midnight.AddHours(15);
        var fourthTime19PM =midnight.AddHours(19);
        var fifthTime21PM =midnight.AddHours(21);

        var allTimes = new [] {firstTime4AM, secondTime12PM, thirdTime15PM, fourthTime19PM, fifthTime21PM};

        var at2AM = midnight.AddHours(2); 
        var at3AM = midnight.AddHours(3);

        var at6AM =  midnight.AddHours(6);
        var at8AM =  midnight.AddHours(8);

        var at13PM =  midnight.AddHours(13);
        var at14PM =  midnight.AddHours(14);

        var at16PM =  midnight.AddHours(16);
        var at18PM =  midnight.AddHours(18);

        var at1930PM =  midnight.AddHours(19).AddMinutes(30);
        var at2030PM =  midnight.AddHours(20).AddMinutes(30);

        var at22PM =  midnight.AddHours(22);
        var at23PM =  midnight.AddHours(23);

        yield return new TestCaseData(new DateTime[] {}, midnight).Returns(0);
        yield return new TestCaseData(new [] {firstTime4AM, secondTime12PM}, midnight).Returns(0);
        yield return new TestCaseData(new [] {firstTime4AM}, at2AM).Returns(0.5/12.0);
        yield return new TestCaseData(new [] {firstTime4AM, secondTime12PM}, at2AM).Returns(0.5/12.0);
        yield return new TestCaseData(new [] {firstTime4AM}, at3AM).Returns(0.75/12.0);

        yield return new TestCaseData(new DateTime[] {}, firstTime4AM).Returns(0);
        yield return new TestCaseData(allTimes, firstTime4AM).Returns(1.0/12.0);
        yield return new TestCaseData(allTimes, at6AM).Returns(0.25/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at8AM).Returns(0.5/6.0 + 1.0/12.0);

        yield return new TestCaseData(new DateTime[] {}, secondTime12PM).Returns(0);
        yield return new TestCaseData(allTimes, secondTime12PM).Returns(1.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at13PM).Returns(1/18.0 + 1.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at14PM).Returns(2/18.0 + 1.0/6.0 + 1.0/12.0);

        yield return new TestCaseData(new DateTime[] {}, thirdTime15PM).Returns(0);
        yield return new TestCaseData(allTimes, thirdTime15PM).Returns(2.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at16PM).Returns(0.25/6.0 + 2.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at18PM).Returns(0.75/6.0 + 2.0/6.0 + 1.0/12.0);

        yield return new TestCaseData(new DateTime[] {}, fourthTime19PM).Returns(0);
        yield return new TestCaseData(allTimes, fourthTime19PM).Returns(3.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at1930PM).Returns(0.25/6.0 + 3.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at2030PM).Returns(0.75/6.0 + 3.0/6.0 + 1.0/12.0);

        yield return new TestCaseData(new DateTime[] {}, fifthTime21PM).Returns(0);
        yield return new TestCaseData(allTimes, fifthTime21PM).Returns(4.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at22PM).Returns(1/36.0 + 4.0/6.0 + 1.0/12.0);
        yield return new TestCaseData(allTimes, at23PM).Returns(2/36.0 + 4.0/6.0 + 1.0/12.0);
      }
    }
  }
}