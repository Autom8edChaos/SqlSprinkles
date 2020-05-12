using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace SqlSprinkles.UnitTest
{
    public class BasicParameterReplacement
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void WhenParameterIsFoundThenItIsReplacedCorrectly()
        {
            var sqlText = "     SET @ProductName = 'foo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {{ "@ProductName", "Fancy Laptop" }});

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Contain("     SET @ProductName = 'Fancy Laptop'");
        }

        [Test]
        public void WhenParameterIsFoundThenItIsReplacedWithOtherParamsIgnored()
        {
            var sqlText = @"
                SET @FirstParam = 'abc'
                SET @SecondParam = 'qwerty'
                SET @ThirdParam = xyz";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "@SecondParam", "Fancy Laptop" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Contain("SET @FirstParam = 'abc'");
            actual.Should().Contain("SET @SecondParam = 'Fancy Laptop'");
            actual.Should().Contain("SET @ThirdParam = xyz");
        }

        [Test]
        public void WhenValueIsNumericThenNoQuotesAreUsed()
        {
            var sqlText = @"
                SET @Amount1 = 1
                SET @Amount2 = 2
                SET @Amount3 = 3
                SET @Amount4 = 4
                SET @Amount5 = 5";


            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {
                { "@Amount1", "10" },
                { "@Amount2", "0" },
                { "@Amount3", "-10" },
                { "@Amount4", "3.14" },
                { "@Amount5", "2.9e4" }
            });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().ContainAll(
                "SET @Amount1 = 10",
                "SET @Amount2 = 0",
                "SET @Amount3 = -10",
                "SET @Amount4 = 3.14",
                "SET @Amount5 = '2.9e4'");
        }
    }
}