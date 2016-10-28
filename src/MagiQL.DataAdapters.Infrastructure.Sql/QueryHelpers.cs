using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using SqlModeller.Model;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public static class QueryHelpers
    {
        public static bool IsCalculatedColumn(string fieldName)
        {
            if (fieldName.Contains('/') 
                || fieldName.Contains('*') 
                || fieldName.Contains('+') 
                || fieldName.Contains('-') 
                || fieldName.Contains("CASE") 
                || fieldName.Contains('>')  
                )
            {
                return true;
            }
            return false;
        }

        public static bool IsCalculatedColumnCompiled(string fieldName)
        {
            return (fieldName.Contains("CASE") 
                    || fieldName.Contains("DATEPART") 
                    || fieldName.Contains("CONCAT") 
                    || fieldName.Contains("dbo.")
                    );
        }

        public static Compare GetOperator(MappedSearchRequestFilter filterByColumn)
        {
            var opertorStr = SqlModeller.Model.Compare.Equal;
            switch (filterByColumn.Mode)
            {
                case FilterModeEnum.Equal:
                    opertorStr = SqlModeller.Model.Compare.Equal;
                    break;
                case FilterModeEnum.GreaterThan:
                    opertorStr = SqlModeller.Model.Compare.GreaterThan;
                    break;
                case FilterModeEnum.GreaterThanOrEqual:
                    opertorStr = SqlModeller.Model.Compare.GreaterThanOrEqual;
                    break;
                case FilterModeEnum.LessThan:
                    opertorStr = SqlModeller.Model.Compare.LessThan;
                    break;
                case FilterModeEnum.LessThanOrEqual:
                    opertorStr = SqlModeller.Model.Compare.LessThanOrEqual;
                    break;
                case FilterModeEnum.NotEqual:
                    opertorStr = SqlModeller.Model.Compare.NotEqual;
                    break;
            }
            return opertorStr;
        } 
 
        public static string DateString(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static Aggregate GetAggregate(FieldAggregationMethod aggregationMethod)
        {
            switch (aggregationMethod)
            { 
                case FieldAggregationMethod.First:
                case FieldAggregationMethod.Min:
                    return Aggregate.Min;

                case FieldAggregationMethod.Max:
                    return Aggregate.Max;
                    break;
                case FieldAggregationMethod.Average:
                    return Aggregate.Avg;
                    break;
                case FieldAggregationMethod.Sum:
                    return Aggregate.Sum;
                    break;
                case FieldAggregationMethod.Exclude:
                    return Aggregate.None;
                    break;
                case FieldAggregationMethod.Bit:
                    return Aggregate.Bit;
                    break;
                case FieldAggregationMethod.BitMax:
                    return Aggregate.BitMax;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("aggregationMethod");
            }
        }
        
        public static List<ReportColumnMapping> GetAllColumnsInFilters(List<MappedSearchRequestFilter> filters)
        {
            var result = new List<ReportColumnMapping>();
            if (filters != null && filters.Any())
            {
                foreach (var f in filters)
                {
                    if (!result.Any(x => x.Id == f.Column.Id))
                    {
                        result.Add(f.Column);
                    }
                } 
            }
              
            return result;
        }

        public static bool FieldNameContainsAggregate(string fieldName)
        {
            string[] aggregateMethods = new[] {"SUM", "MIN", "MAX", "AVG", "COUNT"};
            return aggregateMethods.Any(x => fieldName.Contains(x + "("));
        }
    }
}
