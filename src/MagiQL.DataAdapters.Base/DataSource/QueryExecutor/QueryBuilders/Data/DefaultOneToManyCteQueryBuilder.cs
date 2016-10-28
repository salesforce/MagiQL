using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using SqlModeller.Model;
using SqlModeller.Model.Select;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Data
{
    public class DefaultOneToManyCteQueryBuilder : DefaultDataQueryBuilder
    {
        protected string _rootTableName;
        protected string _fromTableName;
        protected string _fromTableNameAlias;
        protected int _primaryKeyColumnId;
        protected List<string> _foreignKeyColumnNames = new List<string>();
         
        public DefaultOneToManyCteQueryBuilder(IDataSourceComponents dataSourceComponents, string fromKnownTable) : base(dataSourceComponents)
        {  
            var fromTable = dataSourceComponents.TableMappings.GetTableMapping(fromKnownTable);
            _fromTableName = fromKnownTable;
            _fromTableNameAlias = fromTable.Alias;

            // important : if this column is not in the db, it can cause performance issues having to lookup the value all the time, so set cacheOnly:true
            var primaryKeyColumn = _calculatedColumnHelper.FindColumnByFieldName(fromTable.KnownTableName, fromTable.PrimaryKey, FieldAggregationMethod.Exclude, statTransposeKeyValue: null, cacheOnly: true);
            if (primaryKeyColumn != null)
            {
                _primaryKeyColumnId = primaryKeyColumn.Id;
            }
        }

        // Build
        public override void BuildSelect(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping sortColumn, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            base.BuildSelect(query, selectedColumns, sortColumn, groupByColumn, request);

            foreach (var col in _foreignKeyColumnNames)
            {
                if (query.SelectColumns.OfType<ColumnSelector>().All(x => x.Field.Name != col))
                {
                    query.Select(_fromTableNameAlias, col, col, Aggregate.Min);
                }
            } 
        }
        
        public override void BuildFrom(
            SelectQuery query,
            List<ReportColumnMapping> selectedColumns,
            ReportColumnMapping groupByColumn,
            ReportColumnMapping sortByColumn,
            MappedSearchRequest request)
        {

            // idea here is to be able to get a key which can be joined on, so select from the table we're building the CTE from
            // then join on the tables required to get to the join key (which can be used as the group key)

            _rootTableName = request.GroupByColumn.KnownTable;
            var fromTable = _tableMappings.GetTableMapping(_fromTableName);

            var addedTables = new List<string>();

            if (CanJoinTables(_fromTableName, _rootTableName))
            {
                query.From(fromTable.DbTableName, fromTable.Alias);
            }
            else
            {
                var graphBuilder = new TableRelationshipGraphBuilder();
                var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _rootTableName);

                // todo : use path
                //var path = graphBuilder.GetPathFromTableToRoot(relationshipGraph, _fromTableName);
                 
                var distance = graphBuilder.GetDistance(relationshipGraph, _fromTableName);

                query.From(fromTable.DbTableName, fromTable.Alias);

                string currentTableName = fromTable.KnownTableName;
                while (distance > 1)
                {
                    var potentialNodes = graphBuilder.GetByDistance(relationshipGraph, distance - 1);
                    var parentNode = potentialNodes.SingleOrDefault(x => x.Relations.Any(y => y.TableName == currentTableName));
                    if (parentNode == null)
                    {
                        throw new Exception(string.Format("Could not find a relationship between {0} and {1}",currentTableName, relationshipGraph.TableName));
                    }
                    JoinTables(query, parentNode.TableName, currentTableName);
                    currentTableName = parentNode.TableName;
                    distance--;
                    addedTables.Add(parentNode.TableName);
                    if (CanJoinTables(_fromTableName, currentTableName) && CanJoinTables(currentTableName, _rootTableName))
                    {
                        break;
                    }
                }
            }
        }
        
        protected override void BuildGroupBy(SelectQuery query, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            _rootTableName = request.GroupByColumn.KnownTable;

            if (_rootTableName == _fromTableName)
            {
                var table = _tableMappings.GetTableMapping(_fromTableName);
                query.GroupBy(table.Alias, table.PrimaryKey);
                return;
            }

            var joinFromTable = _fromTableName;

            var graphBuilder = new TableRelationshipGraphBuilder();

            if (!CanJoinTables(_fromTableName, _rootTableName))
            {
                // if the table cannot be directly joined to the group by table, work out which join table to group on

                var fromTable = _tableMappings.GetTableMapping(_fromTableName);

                var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _rootTableName);

                var distance = graphBuilder.GetDistance(relationshipGraph, _fromTableName);

                joinFromTable = fromTable.KnownTableName;
                while (distance > 1)
                {
                    var potentialNodes = graphBuilder.GetByDistance(relationshipGraph, distance - 1);
                    var parentNode = potentialNodes.Single(x => x.Relations.Any(y => y.TableName == joinFromTable));
                    joinFromTable = parentNode.TableName;
                    distance--;
                    if (CanJoinTables(_fromTableName, joinFromTable) && CanJoinTables(joinFromTable, _rootTableName))
                    {
                        break;
                    }
                }

            }

            if (CanJoinTables(joinFromTable, _rootTableName))
            {
                var relationship = GetTableRelationships(joinFromTable, _rootTableName).Single();
                if (relationship.Table1.KnownTableName == joinFromTable)
                {
                    query.GroupBy(relationship.Table1.Alias, relationship.Table1Column);
                }
                else if (relationship.Table2.KnownTableName == joinFromTable)
                {
                    query.GroupBy(relationship.Table2.Alias, relationship.Table2Column);
                }
                else
                { 
                    throw new Exception("Failed to select group column");
                }
            }

            else
            {
                throw new Exception(string.Format("Cannot join tables {0} to {1}", joinFromTable, _rootTableName));
            }


        }

        protected override void BuildWhere(SelectQuery query, string queryText, List<MappedSearchRequestFilter> filters, MappedSearchRequest request)
        {
            // not allowed
        }

        protected override void BuildHaving(SelectQuery query, List<MappedSearchRequestFilter> filters, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            // not allowed
        }
        
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            var allColumns = request.SelectedAndDependantColumns;

            // only non claculated columns in this table
            var result = allColumns.Where(x =>
                x.KnownTable == _fromTableName
                && !QueryHelpers.IsCalculatedColumn(x)
                )

            // remove the _C column used for getting group counts
            .Where(x => QueryHelpers.GetFieldName(x) != _constants.CountKeyAlias).ToList();

            if (_primaryKeyColumnId > 0 && result.All(x => x.Id != _primaryKeyColumnId))
            {
                result.Add(QueryHelpers.GetColumnMapping(_primaryKeyColumnId));
            }

            // add the currency key column if its in this table
            if(result.All(x=>x.Id != _constants.CurrencyColumnId))
            { 
                var currencyCol = base.GetCurrencyColumn();
                if (currencyCol != null && currencyCol.KnownTable == _fromTableName)
                {
                    result.Add(currencyCol);
                } 
            }


            return result;
        }
        
        // Override
        protected override bool CanSelectRowNumber()
        {
            return false;
        }
        
        protected override string GetFieldAlias(ReportColumnMapping column, MappedSearchRequest request)
        {
            return column.FieldName;
        }

        protected override string GetFieldAlias(SelectedColumn column)
        {
            var fieldName = QueryHelpers.GetFieldName(column);

            if (Regex.IsMatch(fieldName, "^[0-9.]*$")) // for example fieldName = 1  for getting a count
            {
                fieldName = base.GetFieldAlias(column);
            }

            return fieldName;
        }

        public override ReportColumnMapping GetCurrencyColumn()
        {
            return null;
        }

        public override bool GroupByIsChildTable(string tableAlias, ReportColumnMapping groupByColumn)
        {
            return _dataSourceComponents.QueryBuilderBase.GroupByIsChildTable(tableAlias, groupByColumn);
        }

        public override bool IsDateColumn(ReportColumnMapping x)
        {
            throw new NotImplementedException();
        }

        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            return false;
        }
    }
}
