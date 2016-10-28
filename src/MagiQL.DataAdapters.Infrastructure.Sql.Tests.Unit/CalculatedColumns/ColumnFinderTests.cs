using System.Linq;
using NUnit.Framework;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Tests.Unit.CalculatedColumns
{
    public class ColumnFinderTests
    {

        [TestCase("tbl1.name", "tbl1.name|")]
        [TestCase("MAX(tbl1.name)", "tbl1.name|Max")]
        [TestCase("MAX(tbl1.name)+tbl2.id", "tbl1.name|Max,tbl2.id|")]
        [TestCase("MAX(tbl1.name)+SUM(tbl2.id)", "tbl1.name|Max,tbl2.id|Sum")]
        [TestCase("MAX(tbl1.name)/MIN(tbl2.id)", "tbl1.name|Max,tbl2.id|Min")]
        [TestCase("MAX(tbl1.name)*AVG(tbl2.id)", "tbl1.name|Max,tbl2.id|Average")]
        public void FindColumnNamesInCalculatedFieldWithAggregationMethod_(
            string fieldName,
            string expected)
        {
            var result = new TestCalculatedColumnHelper().FindColumnNamesInCalculatedFieldWithAggregationMethod(fieldName);

            var str = string.Join(",", result.Select(x => x.Item1 + "|" + x.Item2));

            Assert.AreEqual(expected, str);
        }




        [TestCase("Clicks", "Clicks")]
        [TestCase("Clicks / Spend", "Clicks,Spend")]
        [TestCase("(Clicks/Spend) + 20", "Clicks,Spend")]
        [TestCase("(Clicks / Spend) + 20", "Clicks,Spend")]
        [TestCase("dbo.InlineMax(Clicks,Spend) + 20", "Clicks,Spend")]
        [TestCase("CASE WHEN Clicks > 20 THEN Clicks ELSE Spend END", "Clicks,Spend")]
        public void FindColumnNamesInCalculatedField_WithTestCase_ReturnsExpected(
            string input,
            string expectedCsv)
        {
            var expected = expectedCsv.Split(',');

            var result = new TestCalculatedColumnHelper().FindColumnNamesInCalculatedField(input);

            Assert.AreEqual(expected.Count(), result.Count);
            foreach (var exp in expected)
            {
                Assert.IsTrue(result.Contains(exp));
            }
        }
    }
}