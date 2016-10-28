using System;
using System.Collections.Generic;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.SpreadsheetGenerator
{
    public interface ISpreadsheetWriter : IDisposable
    {
        ILoopTimer LoopTimer { get; set; }
        Action<int> ProgressCallback { get; set; }

        void Write(List<ReportColumnMapping> columnDefinitions, List<SearchResultRow> rows, bool writeColumnHeaders);

        void Save(string filepath);
    }
}