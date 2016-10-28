using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler;
using MagiQL.Framework.Interfaces;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Validation
{
    public class DefaultColumnMappingValidator : IReportColumnMappingValidator
    {  
        public virtual bool FieldNameIsValid(string fieldName)
        {
            try
            {
                if (!QueryHelpers.IsCalculatedColumnCompiled(fieldName))
                {
                    var result = new SqlExpressionParser().ConvertToSql(fieldName);
                }

                if (SqlInjectionChecker.HasInjection(fieldName))
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
