using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;
using SpreadsheetLight;

namespace MagiQL.Framework.Renderers.SpreadsheetGenerator
{
    public class SpreadsheetWriter : ISpreadsheetWriter, IDisposable
    {
        private SLDocument Worksheet { get; set; }
        private int rowIndex = 1;

        public ILoopTimer LoopTimer { get; set; }
        public Action<int> ProgressCallback { get; set; }

        public SpreadsheetWriter()
        {
            Worksheet = new SLDocument();
        }

        public void Write(List<ReportColumnMapping> columnDefinitions, List<SearchResultRow> rows, bool writeColumnHeaders)
        {
            if (writeColumnHeaders)
            {
                var firstRow = rows.First();
                WriteColumnHeaders(columnDefinitions, firstRow);
            }

            WriteRows(columnDefinitions, rows);
        }

        private void WriteColumnHeaders(List<ReportColumnMapping> columnDefinitions, SearchResultRow row)
        {
            int columnIndex = 1;
            for (var i = 0; i < row.Values.Count; i++)
            {
                Worksheet.SetCellValue(rowIndex, columnIndex, row.Values[i].Name);
                columnIndex++;
            }
            rowIndex++;
        }

        private void WriteRows(List<ReportColumnMapping> columnDefinitions, List<SearchResultRow> rows)
        {
            int columnIndex;

            int progressPercent = 0;

            var firstRow = rows.First();

            for (var i = 0; i < rows.Count; i++)
            {
                columnIndex = 1;
                for (var j = 0; j < firstRow.Values.Count; j++)
                {
                    var value = rows[i].Values[j].Value;

                    var columnDefinition = columnDefinitions.First(x => x.Id == rows[i].Values[j].ColumnId);

                    string dataFormat = GetDataFormat(columnDefinition);
                    int precision = GetPrecision(columnDefinition);

                    SetCellValue(rowIndex, columnIndex, value, dataFormat, precision);

                    columnIndex++;

                    if (ProgressCallback != null)
                    {
                        progressPercent = (int) ((((double) 100) / rows.Count) * i );
                        ProgressCallback.Invoke(progressPercent);
                        LoopTimer.Loop();
                    }
                }

                rowIndex++;
            }
        }

        private string GetDataFormat(ReportColumnMapping columnDefinition)
        {
            string result = null;
            if (columnDefinition.MetaData!=null)
            {
                result = columnDefinition.MetaData.GetString("DataFormat");
            }
            if (result == null)
            {
                if (columnDefinition.DbType == DbType.Int16
                    || columnDefinition.DbType == DbType.Int32
                    || columnDefinition.DbType == DbType.Int64
                    || columnDefinition.DbType == DbType.Double
                    || columnDefinition.DbType == DbType.Decimal)
                {
                    result = DataFormat.Number;
                }
            }
            return result;
        }

        private int GetPrecision(ReportColumnMapping columnDefinition)
        {
            if (columnDefinition.MetaData != null)
            {
                return columnDefinition.MetaData.GetInt("Precision");
            }
            return 0;
        }

        private void SetCellValue(int rowIndex, int columnIndex, string value, string dataFormat, int precision)
        {
            SetCellID(rowIndex, columnIndex, value, dataFormat);

            SetCellNumber(rowIndex, columnIndex, value, dataFormat);

            SetCellCurrencyOrPercentage(rowIndex, columnIndex, value, dataFormat, precision);

            SetCellString(rowIndex, columnIndex, value, dataFormat);
        }

        #region SetCell

        private void SetCellID(int rowIndex, int columnIndex, string value, string dataFormat)
        {
            if (dataFormat == DataFormat.Id)
            {
                Worksheet.SetCellValue(rowIndex, columnIndex, value.ToString());

                var style = Worksheet.CreateStyle();

                style.FormatCode = "@";

                Worksheet.SetCellStyle(rowIndex, columnIndex, style);
            }
        }

        private void SetCellString(int rowIndex, int columnIndex, string value, string dataFormat)
        {
            if (!(dataFormat == DataFormat.Number || dataFormat == DataFormat.Currency || dataFormat == DataFormat.Percentage || dataFormat == DataFormat.Id))
            {
                Worksheet.SetCellValue(rowIndex, columnIndex, value);
            }
        }

        private void SetCellCurrencyOrPercentage(int rowIndex, int columnIndex, string value, string dataFormat, int precision)
        {
            if (dataFormat == DataFormat.Currency || dataFormat == DataFormat.Percentage)
            {
                SetCellDecimal(rowIndex, columnIndex, value, precision);
            }
        }

        private void SetCellNumber(int rowIndex, int columnIndex, string value, string dataFormat)
        {
            if (dataFormat == DataFormat.Number)
            {
                long number;
                decimal decimalNumber;

                // try first with decimal in case it a 2.00 kind of string
                // we're expecting a long as the format said
                // everything in the decimal part will be dropped (not rounded)
                decimal.TryParse(value, out decimalNumber);
                number = (long)Math.Abs(decimalNumber);

                Worksheet.SetCellValue(rowIndex, columnIndex, number);
            }
        }

        private void SetCellDecimal(int rowIndex, int columnIndex, string value, int precision)
        {
            Worksheet.SetCellValue(rowIndex, columnIndex, Convert.ToDecimal(value));

            var style = Worksheet.CreateStyle();

            style.FormatCode = "0.00";

            if (precision > 0)
            {
                style.FormatCode = "0.";
                for (var k = 0; k < precision; k++)
                {
                    style.FormatCode = style.FormatCode + "0";
                }
            }

            Worksheet.SetCellStyle(rowIndex, columnIndex, style);
        }

        #endregion

        public void Save(string filepath)
        {
            Worksheet.SaveAs(filepath);
        }

        public void Dispose()
        {
            Worksheet.Dispose();
        }



    }
}
