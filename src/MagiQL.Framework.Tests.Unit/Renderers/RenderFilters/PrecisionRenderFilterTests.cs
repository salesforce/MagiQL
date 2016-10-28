using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Renderers.RenderFilters;
using NUnit.Framework;

namespace MagiQL.Framework.Tests.Unit.Renderers.RenderFilters
{
    [TestFixture]
    public class PrecisionRenderFilterTests
    {
        [TestCase("1.123456789", 2, "1.12")]
        [TestCase("1.123456789", 4, "1.1235")]
        [TestCase("1", 2, "1")]
        [TestCase("1.00", 2, "1")]
        [TestCase("1.0001", 2, "1")]
        [TestCase("1.0001", 6, "1.0001")]
        [TestCase("1.01", 2, "1.01")]
        [TestCase("99999999999.11", 2, "99999999999.11")]
        public void ApplyFilter_WithCase_ReturnsExpected(string value, int? precision, string expected)
        {
            var filter = new PrecisionRenderFilter();
            var column = new ReportColumnMapping()
            {
                 MetaData = new List<ReportColumnMetaDataValue>()
                {
                    new ReportColumnMetaDataValue()
                    {
                        Name = "Precision",
                        Value = precision.ToString()
                    }
                } 
            };

            var result = filter.ApplyFilter(value, column, null);

            Assert.AreEqual(expected, result);

        }
    }
}