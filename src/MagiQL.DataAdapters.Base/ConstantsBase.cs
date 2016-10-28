namespace MagiQL.Reports.DataAdapters.Base
{
    public abstract class ConstantsBase
    {
        public abstract int DataSourceId { get; }
        public abstract string Platform { get; }

        public abstract string ConnectionStringName { get; } 

        public abstract string RootTableName { get; }

        public virtual bool FullTextSearchEnabled { get; set; }

        public virtual string GroupKeyAlias { get { return "_GroupKey"; } }
        public virtual string DateKeyAlias { get { return "_DateKey"; } }
        public virtual string CurrencyKeyAlias { get { return "_CurrencyKey"; } }
        public virtual string CountKeyAlias { get { return "_C"; } }
        public virtual string SortKeyAlias { get { return "_SortKey"; } }

        public virtual string DateStatsTableAliasPrefix { get { return "date"; } }
        public virtual string StatsDateDbField { get { return "DateTime"; } }
        
        public virtual string DateStatColumnUniqueName { get { return null; } }
        public virtual int DateStatColumnId { get; set; }

        public virtual string HourStatColumnUniqueName { get { return null; } }
        public virtual int HourStatColumnId { get; set; }

        public virtual string CurrencyColumnUniqueName { get { return null; } }
        public virtual int CurrencyColumnId { get; set; }
        public virtual string TextSearchColumnUniqueName { get { return null; } }
        public virtual int TextSearchColumnId { get; set; }


    }
}