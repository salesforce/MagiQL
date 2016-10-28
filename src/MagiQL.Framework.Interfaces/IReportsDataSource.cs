using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using SqlModeller.Model;

namespace MagiQL.Framework.Interfaces
{
    public interface IReportsDataSource
    {
        /// <summary>
        /// Used to identify which platform the IReportsDataSource implementation is for
        /// </summary>
        string Platform { get; }
        int DataSourceId { get; }

        string ConnectionStringName { get; }

        List<ColumnDefinition> GetAllSelectableColumnDefinitions(int? organizationId, int? groupBy = null);
        List<ReportColumnMapping> GetDependantColumnMappings(int dataSourceId, int columnId); 
        List<ReportColumnMapping> GetDependantColumnMappings(int dataSourceId, string fieldName); 

        IColumnProvider GetColumnProvider();
        IReportColumnMappingValidator GetColumnValidator();
        Query BuildQuery(SearchRequest request, out long MapTime);
        List<ReportColumnMapping> GetColumnMappings(List<SelectedColumn> selectedColumns);
        string GetColumnDisplayName(ColumnDefinition col);
        string GetFieldAlias(ReportColumnMapping col);
        TableInfo GetTableInfo();

        object GetConfiguration();
    }
}
