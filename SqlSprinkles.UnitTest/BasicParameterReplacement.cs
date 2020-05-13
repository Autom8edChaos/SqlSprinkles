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

        [Test]
        public void WhenValueIsBoolThenNoQuotesAreUsed()
        {
            var sqlText = @"
                SET @P1 = TRUE
                SET @P2 = NULL
                SET @P3 = FALSE
                SET @P4 = 'foo'
                SET @P5 = 5";


            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {
                { "@P1", "FALSE" },
                { "@P2", "TRUE" },
                { "@P3", "TRUE" },
                { "@P4", "FALSE" },
                { "@P5", "TRUE" }
            });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().ContainAll(
                "SET @P1 = FALSE",
                "SET @P2 = TRUE",
                "SET @P3 = TRUE",
                "SET @P4 = FALSE",
                "SET @P5 = TRUE");
        }

        [Test]
        public void WhenValueIsNullThenNoQuotesAreUsed()
        {
            var sqlText = @"
                SET @P1 = TRUE
                SET @P2 = 'foo'
                SET @P3 = 5";


            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {
                { "@P1", "NULL" },
                { "@P2", "NULL" },
                { "@P3", "NULL" }
            });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().ContainAll(
                "SET @P1 = NULL",
                "SET @P2 = NULL",
                "SET @P3 = NULL");
        }

        [Test]
        public void WhenQuotesAreUsedThenValueIsTakenAsChar()
        {
            var sqlText = @"
                SET @P1 = TRUE
                SET @P2 = NULL
                SET @P3 = FALSE
                SET @P4 = 'foo'
                SET @P5 = 5";


            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {
                { "@P1", "'TRUE'" },
                { "@P2", "'FALSE'" },
                { "@P3", "'NULL'" },
                { "@P4", "'Text'" },
                { "@P5", "'42'" }
            });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().ContainAll(
                "SET @P1 = 'TRUE'",
                "SET @P2 = 'FALSE'",
                "SET @P3 = 'NULL'",
                "SET @P4 = 'Text'",
                "SET @P5 = '42'");
        }

        [Test]
        public void WhenParamIsWithoutAmpersandThenItGetsUsedAndAdded()
        {
            var sqlText = "SET @ProductName = 'foo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "ProductName", "Fancy Laptop" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Be("SET @ProductName = 'Fancy Laptop' -- 'foo' replaced by 'Fancy Laptop'");
        }

        [Test]
        public void WhenReplacementTextContainQuotesThenTheyAreEscaped()
        {
            var sqlText = "SET @ProductName = 'foo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "ProductName", "O'Reilly's Laptop" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Contain("SET @ProductName = 'O''Reilly''s Laptop' --");
        }

        [Test]
        public void WhenOriginalTextContainQuotesThenTheTextGetReplacedCorrectly()
        {
            var sqlText = "SET @ProductName = 'f''oo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "ProductName", "Fancy Laptop" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Contain("SET @ProductName = 'Fancy Laptop' --");
        }

        [Test]
        public void WhenReplacementTextContainsNvarcharIndicatorThenThisIsUsed()
        {
            var sqlText = "SET @ProductName = 'foo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "ProductName", "N'Fancy Laptop'" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Contain("SET @ProductName = N'Fancy Laptop' --");
        }

        [Test]
        public void WhenUsingNvarcharOptionThenAllStringsBecomeNvarchars ()
        {
            var sqlText = @"
                SET @P1 = N'foo'
                SET @P2 = N'qwerty'
                SET @P3 = 'bar'
                SET @P4 = 'xyz'
                SET @P5 = 10
                SET @P6 = '20'
                SET @P7 = '30'";


            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() {
                { "@P1", "bar" },
                { "@P2", "xizzy" },
                { "@P3", "NULL" },
                { "@P5", "asdf" },
                { "@P6", "'20'" },
                { "@P7", "40" },
            }, ParameterManipulator.ParameterOptions.AlwaysUseNvarchar);

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().ContainAll(
                "SET @P1 = N'bar' --",
                "SET @P2 = N'xizzy' --",
                "SET @P3 = NULL --",
                "SET @P4 = 'xyz'",
                "SET @P5 = N'asdf' --",
                "SET @P6 = N'20' --",
                "SET @P7 = 40 --");
        }

        [Test]
        public void WhenReplacedThenTheReplacementGetCommented()
        {
            var sqlText = "SET @ProductName = 'foo'";
            var paraManipulator = new ParameterManipulator(new Dictionary<string, string>() { { "@ProductName", "Fancy Laptop" } });

            var actual = paraManipulator.Replace(sqlText);
            actual.Should().Be("SET @ProductName = 'Fancy Laptop' -- 'foo' replaced by 'Fancy Laptop'");
        }
    }
}