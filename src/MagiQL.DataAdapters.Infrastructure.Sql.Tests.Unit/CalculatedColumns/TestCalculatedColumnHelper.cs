using System;
using System.Collections.Generic;
using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Tests.Unit.CalculatedColumns
{
    public class TestCalculatedColumnHelper : CalculatedColumnHelperBase
    {
        public TestCalculatedColumnHelper(): base(null, 0)
        {
        } 

        public TestCalculatedColumnHelper(IColumnProvider columnProvider, int dataSourceId) : base(columnProvider, dataSourceId)
        {
        } 

        protected override string GetTableAlias(KeyValuePair<ReportColumnMapping, string> foundColumn, string defaultTable)
        {
            throw new NotImplementedException();
        }

        protected override string GetStatsTable(bool useDateStats)
        {
            throw new NotImplementedException();
        }

        public override string GetFieldAlias(ReportColumnMapping column)
        {
            throw new NotImplementedException();
        }

        protected override ReportColumnMapping FindColumnByFieldName(
            string table,
            string field,
            FieldAggregationMethod aggregationMethod)
        {
            throw new NotImplementedException();
        }
    }
}