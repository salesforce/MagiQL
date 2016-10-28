using System;
using System.Collections.Generic;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Framework.Model.Response
{
    /// <summary>
    /// The response from the search service
    /// </summary>
    public class SearchResponse : SearchResult
    { 
        public SearchRequest Request { get; set; }

        public List<ColumnDefinition> ColumnDefinitions { get; set; }

    }

    /// <summary>
    /// Returned from the search datasource
    /// </summary>
    public class SearchResult : ResponseBase<List<SearchResultRow>>
    { 
        public SearchResultSummary Summary { get; set; }

        public SearchDebugInfo DebugInfo { get; set; }

        public SearchResult()
        {
            Summary = new SearchResultSummary();
            DebugInfo = new SearchDebugInfo();
        }
    }

    public class SearchResultRow
    {
        public string Id { get; set; }
        public List<ResultColumnValue> Values { get; set; }
    }

    
    public class ResultColumnValue
    {
        public int ColumnId { get; set; }
        public string Value { get; set; } 
        public string Name { get; set; } 
        public string Type { get; set; }

        public override string ToString()
        {
            return String.Format("ColumnId: {0} | Name: {1} | Value: {2}", ColumnId, Name, Value);
        }
    }

    public class SearchResultSummary
    { 
        public long MapRequestElapsedMilliseconds { get; set; }
        public long BuildQueryElapsedMilliseconds { get; set; }
        public long CompileQueryElapsedMilliseconds { get; set; }
        public long QueryElapsedMilliseconds { get; set; }
        public long ParseResultElapsedMilliseconds { get; set; }

        public long TotalRows { get; set; }
    }

    public class SearchDebugInfo
    {
        public string SqlQuery { get; set; }

        /// <summary>
        /// The list of warning messages (non-fatal errors) that were generated during query execution. 
        /// 
        /// You should keep an eye on these during development to spot potential bugs in your request.
        /// </summary>
        public ISet<string> WarningMessages { get; private set; }

        public SearchDebugInfo()
        {
            // HashSet so that we don't send loads of duplicate warning message.
            WarningMessages = new HashSet<string>();
        }
    } 
}