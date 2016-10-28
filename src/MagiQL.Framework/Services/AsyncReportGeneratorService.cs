using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Renderers.SpreadsheetGenerator;

namespace MagiQL.Framework.Services
{
    public class AsyncReportGeneratorService : IAsyncReportGeneratorService
    {
        private readonly ISqlQueryExecutor sqlQueryExecutor;
        private readonly IReportsDataSourceFactory _reportsDataSourceFactory;
        private readonly IReportStatusQueryService _reportStatusQueryService;
        private readonly IReportStatusCreationService _reportStatusCreationService;
        private readonly IReportStatusUpdaterService _reportStatusUpdaterService;
        private readonly IRenderFilterService _renderFilterService;
        private readonly ISpreadsheetWriterFactory _spreadsheetWriterFactory;

        /// <summary>
        /// How many rows should be loaded in a single request
        /// </summary>
        internal int _defaultPageSize = 100;
        public int  StatusUpdateTimerInterval = Configuration.Exports.UpdateStatusFrequencySeconds * 1000;

        public AsyncReportGeneratorService(
            ISqlQueryExecutor sqlQueryExecutor,
            IReportsDataSourceFactory reportsDataSourceFactory, 
            IReportStatusQueryService reportStatusQueryService, 
            IReportStatusCreationService reportStatusCreationService, 
            IReportStatusUpdaterService reportStatusUpdaterService,
            IRenderFilterService renderFilterService,
            ISpreadsheetWriterFactory spreadsheetWriterFactory
            )
        {
            this.sqlQueryExecutor = sqlQueryExecutor;
            _reportsDataSourceFactory = reportsDataSourceFactory;
            _reportStatusQueryService = reportStatusQueryService;
            _reportStatusCreationService = reportStatusCreationService;
            _reportStatusUpdaterService = reportStatusUpdaterService;
            _renderFilterService = renderFilterService;
            _spreadsheetWriterFactory = spreadsheetWriterFactory;
        }

        public ReportStatus Setup(string platform, int? organizationId,  int? userId, SearchRequest request, string filePath)
        {
            var newStatus = CreateReportStatus(platform, organizationId, userId);

            // Fire-and-forget. The client will poll to get the current status of this report.

            // TODO: this should be handled in a separate process. If this runs in IIS, the thread
            // we kick off here may be terminated at any time during app pool recycling. This will cause reports to 
            // get mysteriously stuck.

            // Note: threads are GC roots. So it's OK to let the reference to that thread go - it won't get GC'd
            // until it's finished running.
            new Thread(() => ProcessRequestThreadStart(newStatus, request, filePath))
            {
                IsBackground = false
            }
            .Start();

            return newStatus;
        }

        private void ProcessRequestThreadStart(ReportStatus status, SearchRequest request, string filePath)
        {
            try
            {
                // setup
                request.PageSize = _defaultPageSize;
                status.ProgressPercentage = 0;
                status.StatusMessage = "Processing";
                UpdateReportStatus(status);

                var dataSource = _reportsDataSourceFactory.GetDataSource(status.Platform);
                var columnDefinitions = dataSource.GetColumnMappings(request.SelectedColumns);

                var loopTimer = new LoopTimer<ReportStatus>(DateTime.Now, StatusUpdateTimerInterval, x => UpdateReportStatus(x), status);

                // loop through all pages loading the data
                var data = LoadAllData(ref status, request, dataSource, loopTimer);
                
                // save all the data into a spreadsheet
                using (var renderer = _spreadsheetWriterFactory.NewWriter())
                {
                    status.StatusMessage = "Saving"; 
                    UpdateReportStatus(status);

                    renderer.LoopTimer = loopTimer;
                    renderer.ProgressCallback = (i => status.ProgressPercentage = (int)(((double)100 * Configuration.Exports.DataLoadPercent) + ((double) Configuration.Exports.DataSavePercent * i)));

                    renderer.Write(columnDefinitions, data, writeColumnHeaders: true);
                    string fileName = string.Format("report-{0}.xlsx", status.Id);
                    renderer.Save(Path.Combine(filePath, fileName));
                }

                // finish
                status.ProgressPercentage = 100;
                status.DateCompleted = DateTime.Now;
                status.StatusMessage = "Complete";
                UpdateReportStatus(status);
            }
            catch (Exception ex)
            {
                // This runs in a separate thread. We can't throw or the whole process will get killed.
                try
                {
                    status.StatusMessage = "Failed";
                    status.ErrorMessage = ex.Message;
                    status.StackTrace = ex.StackTrace;
                    status.DateCompleted = DateTime.Now;
                    UpdateReportStatus(status);
                }
                catch (Exception e)
                {
                    // TODO: log
                }
            }
        }
        

        private List<SearchResultRow> LoadAllData(ref ReportStatus status, SearchRequest request, IReportsDataSource dataSource, LoopTimer<ReportStatus> loopTimer)
        {
            request.PageIndex = 0;
            request.GetCount = true;
            
            // get the first page, with a count
            var page = GetPage(request, dataSource);

            var totalRows = page.Summary.TotalRows;
            var totalPages = (int) Math.Ceiling((double) totalRows/request.PageSize);

            var result = page.Data;
            
            // getting count could have an impact on performance and is only needed for the first request
            request.GetCount = false; 

            while (request.PageIndex + 1 < totalPages)
            {
                request.PageIndex ++;
                page = GetPage(request, dataSource);
                if (page != null && page.Data != null && page.Data.Any())
                {
                    result.AddRange(page.Data);
                }

                status.ProgressPercentage = (int) Math.Floor(((double) 100/totalPages)*(request.PageIndex + 1)*Configuration.Exports.DataLoadPercent);

                loopTimer.Loop();
            }

            return result;
        }

        private SearchResult GetPage(SearchRequest request, IReportsDataSource dataSource)
        {
            var page = sqlQueryExecutor.Search(dataSource, request);
            if (page != null && page.Data != null && page.Data.Any())
            {
                _renderFilterService.ApplyAllRenderFilters(dataSource, page);
            }
            return page;
        }

        private ReportStatus CreateReportStatus(string platform, int? organizationId, int? userId)
        {
            var result = new ReportStatus
            {
                DateCreated = DateTime.UtcNow,
                DateUpdated= DateTime.UtcNow,
                ProgressPercentage = 0,
                StatusMessage = "Initialising",
                Platform = platform,
                CreatedByUserId = userId,
                OrganizationId = organizationId,
                MachineName = Configuration.Environment.MachineName
            };

            _reportStatusCreationService.InsertReportStatus(result);

            return result;
        } 

        public ReportStatus GetStatus(long requestId)
        {
            return _reportStatusQueryService.GetReportStatus(requestId);
        }

        private void UpdateReportStatus(ReportStatus result)
        {
            result.DateUpdated = DateTime.UtcNow;

            _reportStatusUpdaterService.UpdateReportStatus(result);
        }

        
    }
}
