using System.Collections.Generic;
using System.Linq;

namespace MagiQL.Framework.Model.Columns
{ 
    public class CalculatedReportColumnMappingValues
    { 

        public class CalculatedReportColumnMappingValue
        {
            // values
            public string CalculatedColFieldName { get; set; }
            public string TableName { get; set; }
            public string FieldName { get; set; }

            // scenario 
            public bool dontAggregate { get; set; }
            public bool useFieldAlias { get; set; }
        }

        private List<CalculatedReportColumnMappingValue> Values = new List<CalculatedReportColumnMappingValue>();

        public void Clear()
        {
            Values = new List<CalculatedReportColumnMappingValue>();
        }

        private CalculatedReportColumnMappingValue GetExisting(bool dontAggregate, bool useFieldAlias)
        {
            return Values.FirstOrDefault(x => x.dontAggregate == dontAggregate && x.useFieldAlias == useFieldAlias);
        }

        public string GetCalculatedColFieldName(bool dontAggregate, bool useFieldAlias)
        {
            if (Settings.CalcultedColumnCacheDisabled)
            {
                return null;
            }

            var existing = GetExisting(dontAggregate, useFieldAlias);

            if (existing != null)
            {
                return existing.CalculatedColFieldName;
            }

            return null;
        }


        public void SetCalculatedColFieldName(bool dontAggregate, bool useFieldAlias, string fieldName)
        { 
            var existing = GetExisting(dontAggregate, useFieldAlias);

            if (existing != null)
            {
                existing.CalculatedColFieldName = fieldName;
            }
            else
            {
                Values.Add(new CalculatedReportColumnMappingValue()
                { 
                    dontAggregate = dontAggregate,
                    useFieldAlias = useFieldAlias,
                    CalculatedColFieldName = fieldName
                });
            } 
        } 

        /// <summary>
        /// returns a 2 value array where the first valye is the table name and the second is the field name, used to create a Column object
        /// </summary> 
        /// <param name="dontAggregate"></param>
        /// <returns></returns>
        public string[] GetColumnTableAndField(bool dontAggregate, bool useFieldAlias)
        {
            if (Settings.CalcultedColumnCacheDisabled)
            {
                return null;
            }
             
            var existing = GetExisting(dontAggregate, useFieldAlias);

            if (existing != null && existing.FieldName != null)
            {
                return new[] { existing.TableName, existing.FieldName };
            }

            return null;
        }

        public void SetColumnTableAndField(bool dontAggregate, bool useFieldAlias, string table, string field)
        { 
            var existing = GetExisting(dontAggregate, useFieldAlias);

            if (existing != null)
            {
                existing.TableName = table;
                existing.FieldName = field;
            }
            else
            {
                Values.Add(new CalculatedReportColumnMappingValue()
                { 
                    dontAggregate = dontAggregate,
                    useFieldAlias = useFieldAlias,
                    TableName = table,
                    FieldName = field
                });
            }
        }

    }
}