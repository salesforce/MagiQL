using System;
using System.Linq;
using System.Text.RegularExpressions;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;
using SqlModeller.Model;

namespace MagiQL.Reports.DataAdapters.Base
{
    public class DefaultQueryHelpers
    {
        protected IColumnProvider _columnProvider;

        protected readonly ConstantsBase _constants;
        protected readonly TableMappingsBase _tableMappings;
         
        public DefaultQueryHelpers(IColumnProvider columnProvider, ConstantsBase constants, TableMappingsBase tableMappings)
        {
            _columnProvider = columnProvider;
            _constants = constants;
            _tableMappings = tableMappings;
        }

        #region TableHelpers
        
        public virtual TableType GetTableType(string tableName)
        {
            return _tableMappings.GetTableMapping(tableName).TableType;
        }
        
        public virtual string GetTableAlias(string table)
        {
            return _tableMappings.GetTableAlias(table);
        }

        #endregion

        #region ColumnHelpers

        public virtual ReportColumnMapping GetColumnMapping(int columnId)
        {
            var result = _columnProvider.GetColumnMapping(_constants.DataSourceId, columnId);

            if (result == null)
            {
                throw new Exception("Column not found for column id : " + columnId);
            }

            return result;
        }

        public virtual bool IsCalculatedColumn(ReportColumnMapping column)
        {
            return column.IsCalculated || QueryHelpers.IsCalculatedColumn(column.FieldName);
        } 

        public virtual bool IsStatsColumn(ReportColumnMapping col)
        {
            return _tableMappings.GetTableMapping(col.KnownTable).TableType == TableType.Stats;
        }
        
        public virtual bool IsTransposeStatColumn(ReportColumnMapping column)
        {
            return _tableMappings.GetTableMapping(column.KnownTable) is TransposeStatsTableMapping;
        }

        public int? GetTransposeKeyValue(ReportColumnMapping column)
        {
            return column.ActionSpecId;
        }
         
        public virtual bool IsSelectable(ReportColumnMapping column, ReportColumnMapping groupBy)
        {
            return column.Selectable && (groupBy == null || IsSelectableForGroupBy(column, groupBy));
        }

        public virtual bool IsSelectableForGroupBy(ReportColumnMapping column, ReportColumnMapping groupBy)
        {
            var groupByTable = groupBy.KnownTable;

            if (GetTableType(groupByTable) == TableType.Stats)
            {
                return false;
            }

            return true;
        }
        
        public virtual string GetDisplayName(ColumnDefinition column)
        {
            return column.DisplayName;
        }
         
        public virtual string GetFieldAlias(SelectedColumn column)
        {
            var col = GetColumnMapping(column.ColumnId);
            return GetFieldAlias(col);
        }
          
        public virtual string GetFieldAlias(ReportColumnMapping column)
        {
            // this is just to help us debug sql, we could just use the id alone
            var rgx = new Regex("[^a-zA-Z0-9_]");
            var friendlyName = rgx.Replace(column.UniqueName, "");

            var result = "c" + friendlyName + "_" + column.Id;

            if (column.ActionSpecId > 0 && IsTransposeStatColumn(column))
            {
                result += "_" + column.ActionSpecId;
            }

            return result;
        }

        public virtual string GetFieldName(SelectedColumn column)
        {
            var col = GetColumnMapping(column.ColumnId);

            return GetFieldName(col);
        }

        public virtual string GetFieldName(ReportColumnMapping column)
        {
            return column.FieldName;
        }

        public virtual bool ContainsAggregate(string name)
        {
            var aggregateStrings = Enum.GetNames(typeof(Aggregate)).Select(x => ((Aggregate)Enum.Parse(typeof(Aggregate), x)).ToSqlString());

            return aggregateStrings.Any(agg => name.Contains(agg + "("));
        }

        #endregion
        
    }
}
