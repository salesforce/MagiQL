using MagiQL.DataAdapters.Infrastructure.Sql.Validation;
using NUnit.Framework;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Tests.Unit
{ 
    [TestFixture]
    public class ReportColumnMappingValidatorTests
    {
        // valid
        [TestCase("ID", true)]
        [TestCase("(ID)", true)]
        [TestCase("ID + 4", true)]
        [TestCase("ID * (8 * 4)", true)]
        [TestCase("ID * 10% + COL2", true)]
        [TestCase("ID + CASE WHEN 1 > 2 THEN 1 ELSE 2 END", true)]
        [TestCase("ID + MAXOF(1,2)", true)]

        // invalid
        [TestCase("(ID", false)]
        [TestCase("ID + (4", false)]
        [TestCase("ID ** (8 * 4)", false)]
        [TestCase("ID * ", false)]
        [TestCase("ID )", false)]

        // sql injection
        [TestCase("ID /* */", false)]
        [TestCase("ID -- 1", false)]
        [TestCase("ID select ", false)]
        [TestCase("ID + drop ", false)]
        public void FieldNameIsValid_WithCase_ReturnsExpected(string input, bool expected)
        {
            var validator = new DefaultColumnMappingValidator();
            var result = validator.FieldNameIsValid(input);

            Assert.AreEqual(expected, result);
        } 
    }
}
