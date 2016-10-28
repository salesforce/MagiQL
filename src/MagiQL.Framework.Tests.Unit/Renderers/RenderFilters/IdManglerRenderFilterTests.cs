using System.Collections.Generic;
using System.Data;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Renderers;
using NUnit.Framework;

namespace MagiQL.Framework.Tests.Unit.Renderers.RenderFilters
{
    [TestFixture]
    public class DataFormatRenderFilterTests
    {
        [TestCase("1.12", "Percent", "1.12")]
        [TestCase("1.123456789", "Percent", "1.123456789")]
        [TestCase("99999", "Percent", "99999")]
//
//        [TestCase("22", "MangledInt", "sx2ug5")]
//        [TestCase("123456", "MangledInt", "sx1zar")]
//
//        [TestCase("22", "MangledLong", "dde03qevg0rl1")]
//        [TestCase("123456", "MangledLong", "dde03qevgzwfn")]
//        [TestCase("123456789123456", "MangledLong", "dde0k305ibo6b")]

        [TestCase(null, "Boolean", "False")]
        [TestCase("0", "Boolean", "False")]
        [TestCase("1", "Boolean", "True")]
        public void ApplyFilter_WithCase_ReturnsExpected(string value, string format, string expected)
        {
            var filters = new RenderFilterFactory().GetFilters();
            var column = new ReportColumnMapping()
            {
                MetaData = new List<ReportColumnMetaDataValue>()
                {
                    new ReportColumnMetaDataValue()
                    {
                        Name = "DataFormat",
                        Value = format
                    }
                }
            };

            if (format == "Boolean")
            {
                column.DbType = DbType.Boolean;
            }

            var result = value;
            filters.ForEach(x=> result = x.ApplyFilter(result, column, null));

            Assert.AreEqual(expected, result);

        }
    }
}