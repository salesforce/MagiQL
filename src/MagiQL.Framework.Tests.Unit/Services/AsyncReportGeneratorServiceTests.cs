using System;
using System.Collections.Generic;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Renderers.SpreadsheetGenerator;
using MagiQL.Framework.Services;
using Moq;
using NUnit.Framework;

namespace MagiQL.Framework.Tests.Unit.Services
{
    [TestFixture]
    public class AsyncReportGeneratorServiceTests
    {
        public AsyncReportGeneratorService Service { get; set; }

        private Mock<ISqlQueryExecutor> _sqlQueryExecutorMock;
        private Mock<IReportsDataSourceFactory> _reportsDataFactoryMock;
        private Mock<IReportsDataSource> _reportsDataSourceMock;
        private Mock<IReportStatusQueryService> _reportsStatusQueryServiceMock;
        private Mock<IReportStatusCreationService> _reportsStatusCreationServiceMock;
        private Mock<IReportStatusUpdaterService> _reportsStatusUpdaterServiceMock;
        private Mock<IRenderFilterService> _renderFilterServiceMock;
        private Mock<ISpreadsheetWriterFactory> _spreadsheetWriterFactoryMock;
        private Mock<ISpreadsheetWriter> _spreadsheetWriterMock;

        private string platform = "Facebook";
        private int organizationId = 1;
        private int userId = -1;
        private string filePath = "c:\\Temp";

        private SearchRequest searchRequest;

        private ReportStatus _CreatedStatus;

        [SetUp]
        public void Setup()
        {
            _sqlQueryExecutorMock = new Mock<ISqlQueryExecutor>();
            _reportsDataFactoryMock = new Mock<IReportsDataSourceFactory>();
            _reportsDataSourceMock = new Mock<IReportsDataSource>();
            _reportsStatusQueryServiceMock = new Mock<IReportStatusQueryService>();
            _reportsStatusCreationServiceMock = new Mock<IReportStatusCreationService>();
            _reportsStatusUpdaterServiceMock = new Mock<IReportStatusUpdaterService>();
            _renderFilterServiceMock = new Mock<IRenderFilterService>();
            _spreadsheetWriterFactoryMock = new Mock<ISpreadsheetWriterFactory>();
            _spreadsheetWriterMock = new Mock<ISpreadsheetWriter>();

            Service = new AsyncReportGeneratorService(
                _sqlQueryExecutorMock.Object,
                _reportsDataFactoryMock.Object,
                _reportsStatusQueryServiceMock.Object,
                _reportsStatusCreationServiceMock.Object,
                _reportsStatusUpdaterServiceMock.Object,
                _renderFilterServiceMock.Object,
                _spreadsheetWriterFactoryMock.Object
                );


            _reportsStatusCreationServiceMock.Setup(x => x.InsertReportStatus(It.IsAny<ReportStatus>()))
                .Callback<ReportStatus>(x => _CreatedStatus = x);
             
            _reportsStatusUpdaterServiceMock.Setup(x => x.UpdateReportStatus(It.IsAny<ReportStatus>()))
                .Callback<ReportStatus>(x => _CreatedStatus = x);

            _reportsDataFactoryMock.Setup(x => x.GetDataSource(It.IsAny<string>()))
                .Returns(_reportsDataSourceMock.Object);

            _spreadsheetWriterFactoryMock.Setup(x => x.NewWriter())
                .Returns(_spreadsheetWriterMock.Object);

            searchRequest = new SearchRequest(){ PageSize =  10};
        }

        private void SetupQueryExecutorMock(int totalRows)
        {
            _sqlQueryExecutorMock.Setup(x => x.Search(It.IsAny<IReportsDataSource>(), It.IsAny<SearchRequest>(), It.IsAny<bool>()))
                .Returns<IReportsDataSource, SearchRequest, bool>((source, request, doNotExecute) =>
                {
                    var result =  new SearchResult()
                    {
                        Data = new List<SearchResultRow>()
                        {
                            new SearchResultRow() {}
                        }
                    };

                    result.Summary.TotalRows = totalRows;

                    return result;
                });
        }

        [Test]
        public void Setup_CreatesStatusAndStartsProcessing()
        {
            // creates report status 
            // calls processrequest async
            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath); 

            _reportsStatusCreationServiceMock.Verify(x => x.InsertReportStatus(It.IsAny<ReportStatus>()), Times.Once());

            Assert.IsNotNull(_CreatedStatus);
            Assert.AreEqual(platform, _CreatedStatus.Platform);
            Assert.AreEqual(userId, _CreatedStatus.CreatedByUserId);
            Assert.AreEqual(organizationId, _CreatedStatus.OrganizationId);
            Assert.AreEqual(0, _CreatedStatus.ProgressPercentage); 
        }
        
        [Test]
        public void Setup_ReturnsStatusBeforeFinished()
        {
            // when long running request
            // returns quickly          
              
            _sqlQueryExecutorMock.Setup(x => x.Search(It.IsAny<IReportsDataSource>(), It.IsAny<SearchRequest>(), It.IsAny<bool>()))
                .Returns<IReportsDataSource, SearchRequest>((source, request) =>
                {
                    // simulate a long running process
                    System.Threading.Thread.Sleep(1000);
                    return new SearchResult();
                });

            DateTime startDate = DateTime.Now;
            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);
            DateTime endDate = DateTime.Now;

            Assert.AreNotEqual(100,result.ProgressPercentage);
            Assert.Less(endDate.Subtract(startDate).TotalMilliseconds, 1000);  
        }
        
        [Test]
        public void LoadData_Paginates()
        {
            // should make multiple search calls incrementing the page number each time
            Service._defaultPageSize = 1;

            SetupQueryExecutorMock(totalRows: 10);

            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);

            System.Threading.Thread.Sleep(100); // let it finish processing

            _sqlQueryExecutorMock.Verify(x => x.Search(It.IsAny<IReportsDataSource>(), It.IsAny<SearchRequest>(), It.IsAny<bool>()), Times.Exactly(10));

        }

        [Test]
        public void LoadData_AppliesRenderFilters()
        {
            // should make multiple search calls incrementing the page number each time
            Service._defaultPageSize = 1;

            SetupQueryExecutorMock(totalRows: 10);

            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);

            System.Threading.Thread.Sleep(100); // let it finish processing

            _renderFilterServiceMock.Verify(x => x.ApplyAllRenderFilters(It.IsAny<IReportsDataSource>(), It.IsAny<SearchResult>()), Times.Exactly(10));
        }
        
        [Test]
        public void ProcessRequestAsync_SetsStatusToInProgress()
        {
            // should make multiple search calls incrementing the page number each time
            Service._defaultPageSize = 1;

            SetupQueryExecutorMock(totalRows: 10);

            bool hasPercentageSet = false;

            _reportsStatusUpdaterServiceMock.Setup(x => x.UpdateReportStatus(It.IsAny<ReportStatus>())).Callback<ReportStatus>(status =>
            {
                if (status.ProgressPercentage > 0 && status.ProgressPercentage < 100)
                {
                    hasPercentageSet = true;
                }
            } );

            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);
             
            System.Threading.Thread.Sleep(1000); // let it finish processing

            Assert.True(hasPercentageSet);
        }
        
        [Test]
        public void ProcessRequestAsync_WithSuccess_SetsStatusToComplete()
        {
            // all fine
            // status should be set to complete
            // should make multiple search calls incrementing the page number each time
            Service._defaultPageSize = 1;

            SetupQueryExecutorMock(totalRows: 10);

            bool isComplete = false;

            _reportsStatusUpdaterServiceMock.Setup(x => x.UpdateReportStatus(It.IsAny<ReportStatus>())).Callback<ReportStatus>(status =>
            {
                if (status.ProgressPercentage == 100)
                {
                    isComplete = true;
                }
            });

            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);

            System.Threading.Thread.Sleep(1000); // let it finish processing

            Assert.True(isComplete);  
        } 

        [Test]
        public void ProcessRequestAsync_WithException_SetsStatusToFailed()
        {
            // all fine
            // status should be set to complete
            // should make multiple search calls incrementing the page number each time
            Service._defaultPageSize = 1;

            _spreadsheetWriterMock.Setup(x => x.Write(It.IsAny<List<ReportColumnMapping>>(), It.IsAny<List<SearchResultRow>>(), It.IsAny<bool>()))
                .Callback<List<ReportColumnMapping>, List<SearchResultRow>, bool>((x, y, z) =>
                {
                    throw new Exception("BOOM!!!");
                });

            SetupQueryExecutorMock(totalRows: 10);

            bool hasFailed = false;

            _reportsStatusUpdaterServiceMock.Setup(x => x.UpdateReportStatus(It.IsAny<ReportStatus>())).Callback<ReportStatus>(status =>
            {
                if (status.StatusMessage == "Failed")
                {
                    hasFailed = true;
                }
            });

            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);

            System.Threading.Thread.Sleep(1000); // let it finish processing

            Assert.True(hasFailed);
        }
        
        [Test]
        public void ProcessRequestAsync_WritesToSpreadsheet()
        {
            // loads data
            // writes to the spreadsheet renderer 
            // calls save

            Service._defaultPageSize = 1;

            SetupQueryExecutorMock(totalRows: 10);

            bool isComplete = false;
            
            var result = Service.Setup(platform, organizationId, userId, searchRequest, filePath);

            System.Threading.Thread.Sleep(100); // let it finish processing
            
            _spreadsheetWriterMock.Verify(x=>x.Write(It.IsAny<List<ReportColumnMapping>>(), It.IsAny<List<SearchResultRow>>(), It.IsAny<bool>()), Times.Once);
            _spreadsheetWriterMock.Verify(x => x.Save(It.IsRegex(filePath.Replace("\\","\\\\"))), Times.Once);

        }

    }

    

}