using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Model.Response.Base;
using MagiQL.Service.Interfaces;
using MagiQL.Service.Client;
using System.Data;

namespace MagiQL.DataExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportsService _reportsService;

        public HomeController()
        {
            _reportsService = new ReportsServiceClient();
        }
         
        public ActionResult Index(string platform = null)
        {
            int orgId = ImpersonateController.GetOrgId(Request);

            if (platform == null)
            {
                return SelectProvider();
            }

            ViewBag.Platform = platform;
            ViewBag.OrganizationId = orgId;
            return View();
        }

        [HttpPost]
        public ActionResult Search(string platform, string q, int group, int sort, bool sortDesc, DateTime? startDate, DateTime? endDate, TemporalAggregation temporalAggregation, DateRangeType dateRangeType, int[] status, int[] column, int pageIndex, int pageSize, bool debug, bool excludeRecordsWithNoStats, int summarize = 0, bool export = false, int orgId = 0)
        { 
            if (column == null || !column.Any())
            {
                return View(new SearchResponse{Error = new ResponseError(){Message = "No Columns Selected"}} );
            } 

            var filters = new List<SearchRequestFilter>(); 
            LoadFilters(filters);
            var textFilterColumns = LoadTextColumns();

            var selectedCols = new List<SelectedColumn>();

            if (column == null || !column.Any())
            {
                throw new Exception("No Columns Selected");
            }

            foreach (var col in column)
            {
                var sCol = new SelectedColumn(col); 
                selectedCols.Add(sCol);
            }  

            var searchRequest = new SearchRequest()
            {
                GetCount = pageIndex == 0,
                Query = q,
                PageIndex = pageIndex,
                PageSize = pageSize,
                GroupByColumn = new SelectedColumn(group),
                SummarizeByColumn= summarize > 0 ? new SelectedColumn(summarize) : null,
                SortByColumn = new SelectedColumn(sort),
                SortDescending = sortDesc,
                SelectedColumns = selectedCols,
                Filters = filters,
                TemporalAggregation = temporalAggregation,
                ExcludeRecordsWithNoStats = excludeRecordsWithNoStats,
                DebugMode = debug,
                DateRangeType = dateRangeType,
                TextFilterColumns = textFilterColumns
            };
             
            searchRequest.DateStart = startDate;
            searchRequest.DateEnd = endDate;

            if (dateRangeType == DateRangeType.Utc)
            {
                // In our test UI, we ask the user to enter a UTC date so no User timezone to UTC conversion required here. 
                if (searchRequest.DateStart.HasValue) searchRequest.DateStart = DateTime.SpecifyKind(searchRequest.DateStart.Value, DateTimeKind.Utc);
                if (searchRequest.DateEnd.HasValue) searchRequest.DateEnd = DateTime.SpecifyKind(searchRequest.DateEnd.Value, DateTimeKind.Utc);
            }
            else
            {
                // The date range is in Account Time, i.e. the dates are not instants in time. They're just a day and a time in some unspecified timezone.
                if (searchRequest.DateStart.HasValue) searchRequest.DateStart = DateTime.SpecifyKind(searchRequest.DateStart.Value, DateTimeKind.Unspecified);
                if (searchRequest.DateEnd.HasValue) searchRequest.DateEnd = DateTime.SpecifyKind(searchRequest.DateEnd.Value, DateTimeKind.Unspecified);
            }
           
            if (export)
            {
                return Export(platform, searchRequest);
            } 
            
            var sw = new Stopwatch();
            sw.Start();

            var result = _reportsService.Search(platform, orgId, Configuration.UserId, searchRequest);

            sw.Stop();
            ViewBag.SearchElapsedMilliseconds = sw.ElapsedMilliseconds;

            return View(result);
        }

        private List<SelectedColumn> LoadTextColumns()
        {
            var textFields = Request["textField"];

            if (!string.IsNullOrEmpty(textFields))
            {
                var textColumns = new List<SelectedColumn>();
                var textFieldsList = textFields.Split(',');

                for (int i = 0; i < textFieldsList.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(textFieldsList[i]))
                    {
                        textColumns.Add(new SelectedColumn() { ColumnId = int.Parse(textFieldsList[i]) });
                    }
                }

                return textColumns;
            }

            return null;
        }

        private void LoadFilters(List<SearchRequestFilter> filters)
        {
            var filterFields = Request["filterField"];
            var filterModes = Request["filterMode"];
            var filterValues = Request["filterValue"];

            if (!string.IsNullOrEmpty(filterFields) && !string.IsNullOrEmpty(filterModes) && !string.IsNullOrEmpty(filterValues))
            {
                var filterFieldsList = filterFields.Split(',');
                var filterModesList = filterModes.Split(',');
                var filterValuesList = filterValues.Split(',');

                for (int i = 0; i < filterFieldsList.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(filterFieldsList[i]) && !string.IsNullOrEmpty(filterModesList[i]) && !string.IsNullOrEmpty(filterValuesList[i]))
                    {
                        filters.Add(new SearchRequestFilter()
                        {
                            ColumnId = int.Parse(filterFieldsList[i]),
                            Mode = GetFilterMode(filterModesList[i]),
                            Value = filterValuesList[i]
                        }); 
                    }
                }  
            }
        }

        public ActionResult Export(string platform, SearchRequest searchRequest)
        {
            int orgId = ImpersonateController.GetOrgId(Request);
            int userId = ImpersonateController.GetUserId(Request);

            var sw = new Stopwatch();
            sw.Start();

            var result = _reportsService.GenerateReport(platform, orgId, userId, searchRequest);

            sw.Stop();
            ViewBag.SearchElapsedMilliseconds = sw.ElapsedMilliseconds;

            return View("Export", result);
        }
          
        private FilterModeEnum? GetFilterMode(string filterMode)
        {
            switch (filterMode)
            {
                case "<":
                    return FilterModeEnum.LessThan;
                case "<=":
                    return FilterModeEnum.LessThanOrEqual;
                case "=":
                    return FilterModeEnum.Equal;
                case ">=":
                    return FilterModeEnum.GreaterThanOrEqual;
                case ">":
                    return FilterModeEnum.GreaterThan;
                case "!=":
                    return FilterModeEnum.NotEqual;
            } 
            return FilterModeEnum.GreaterThan;
        }


        public ActionResult SearchForm(string platform)
        {
            int orgId = ImpersonateController.GetOrgId(Request);
            int userId = ImpersonateController.GetUserId(Request);
            ViewBag.OrganizationId = orgId;

            var columns = _reportsService.GetSelectableColumns(platform, orgId, userId);

            var model = columns.Data.OrderBy(x=> x.MainCategory == "Date" ? 1 : 2).ThenBy(x=>x.Id).ToList();

            var idColulmn = columns.Data.FirstOrDefault(x => x.UniqueName.ToLower().EndsWith("id") || x.DisplayName.ToLower().EndsWith("id"));
            if (idColulmn != null)
            {
                ViewBag.SelectedColumnName = idColulmn.UniqueName;
            }

            // get the text columns for text filter column selector

            var columnMappings = _reportsService.GetColumnMappings(platform, orgId, userId);
            var textColumns = columnMappings.Data.Where(x => IsStringType(x.DbType));

            ViewBag.TextColumnIds = textColumns.Select(x => x.Id.ToString()).ToList();

            return PartialView("SearchForm", model);
        }

        public static bool IsStringType(DbType value)
        {
            switch (value)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return true;
            }

            return false;
        }

        public ActionResult SelectProvider()
        {
            return View("SelectProvider");
        }

        public ActionResult Platforms()
        {
            var model = _reportsService.GetPlatforms().Data;
            return View(model);
        }

    }
}
