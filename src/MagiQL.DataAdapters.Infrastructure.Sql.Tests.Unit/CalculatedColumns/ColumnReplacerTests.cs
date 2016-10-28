using System.Collections.Generic;
using NUnit.Framework;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Tests.Unit.CalculatedColumns
{
    public class ColumnReplacerTests
    {
        [TestCase("NoMatch", "Clicks", "MyClicks", "NoMatch")]
        [TestCase("Clicks", "Clicks", "MyClicks", "MyClicks")]
        [TestCase("Clicks + Spend", "Clicks", "MyClicks", "MyClicks + Spend")]
        [TestCase("Clicks + ClicksPlus", "Clicks", "MyClicks", "MyClicks + ClicksPlus")]
        [TestCase("Clicks + PlusClicks", "Clicks", "MyClicks", "MyClicks + PlusClicks")]
        public void ReplaceFieldName_WithTestCase_ReturnsExpected(string input, string oldName, string newName, string expected)
        {
            var result = new TestCalculatedColumnHelper().ReplaceFieldName(new Stack<string>(), input, oldName, newName);

            Assert.AreEqual(expected, result);
        } 
         

    }
}
