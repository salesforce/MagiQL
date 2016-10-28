namespace MagiQL.Service.Client.Tests.Manual
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Model.Columns;
    using Framework.Model.Request;
    using NUnit.Framework;

    [Category("Reports"), Category("Client"), Category("Integration")]
    [TestFixture]
    public class ReportsServiceClientTests
    {
        private string platform = "facebook";

        private ReportsServiceClient client;

        [SetUp]
        public void Setup()
        {
            this.client = new ReportsServiceClient();
        }

        [Test]
        public void Search()
        {
            var request = new SearchRequest()
            {
                GroupByColumn = new SelectedColumn(101), // campaignid
                SortByColumn = new SelectedColumn(101), // campaignid
                PageSize = 20,
                SelectedColumns = new List<SelectedColumn> { new SelectedColumn(101) }// campaignid
            };

            var result = this.client.Search(this.platform, 1, null, request);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.Greater(result.Data.Count, 0);
        }

        #region Columns

        [Test]
        public void GetSelectableColumns()
        {
            var result = this.client.GetSelectableColumns(this.platform, 1);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.Greater(result.Data.Count, 0);
        }

        [Test]
        public void GetSelectableColumns_HasMetadata()
        {
            var result = this.client.GetSelectableColumns(this.platform, 1);

            Assert.NotNull(result);
            Assert.Null(result.Error); 

            var withMeta = result.Data.Where(x=>x.MetaData!=null && x.MetaData.Any());
            Assert.Greater(withMeta.Count(), 0);
        }
    


        [Test]
        public void GetColumnMappings()
        {
            var result = this.client.GetColumnMappings(this.platform, 1, null);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.Greater(result.Data.Count, 0);
        }

        [Test]
        public void UpdateColumnMapping()
        {
            var create = this.client.GetColumnMappings(this.platform, 1, null).Data.First();
            create.Id = 0;
            create.OrganizationId = -999;
            create.MainCategory = "Unit Test";
            create.DisplayName = "Unit Test : UpdateColumnMapping (Before)" + DateTime.Now;
            create.UniqueName = create.DisplayName;

            var existing = this.client.CreateColumnMapping(this.platform, create);

            var update = existing.Data;

            update.DisplayName = "Unit Test : UpdateColumnMapping (After)" + DateTime.Now;

            var result = this.client.UpdateColumnMapping(this.platform, existing.Data.Id, null, update);
            
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.AreEqual(result.Data.DisplayName, update.DisplayName);
            
            var reload = this.client.GetColumnMappings(this.platform, -999, null);
            
            Assert.NotNull(reload);
            Assert.Null(reload.Error);
            Assert.NotNull(reload.Data);
            Assert.AreEqual(reload.Data.First(x=>x.Id == update.Id).DisplayName, update.DisplayName);
        }


        [Test]
        public void UpdateColumnMappingMetaData()
        {
            var create = this.client.GetColumnMappings(this.platform, 1, null).Data.First();
            create.Id = 0;
            create.OrganizationId = -999;
            create.MainCategory = "Unit Test";
            create.DisplayName = "Unit Test : UpdateColumnMappingMetaData (Before)" + DateTime.Now;
            create.UniqueName = create.DisplayName;


            create.MetaData.Add(new ReportColumnMetaDataValue() { Name = "ExistingMetaData", Value = "Before" });

            var existing = this.client.CreateColumnMapping(this.platform, create);

            var update = existing.Data;

            Assert.AreEqual(1, update.MetaData.Count(x=>x.Name == "ExistingMetaData"));

            update.MetaData.First(x => x.Name == "ExistingMetaData").Value = "After";
            update.MetaData.Add(new ReportColumnMetaDataValue(){Name = "NewMetaData", Value = "New"});

            var result = this.client.UpdateColumnMapping(this.platform, existing.Data.Id, null, update); 
             
            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.AreEqual(result.Data.MetaData.First().Name, update.MetaData.First().Name);

            var reload = this.client.GetColumnMappings(this.platform, -999, null);

            Assert.NotNull(reload);
            Assert.Null(reload.Error);
            Assert.NotNull(reload.Data); 
            Assert.AreEqual(reload.Data.First(x => x.Id == update.Id).MetaData.First(x=>x.Name== "ExistingMetaData").Value, "After");
            Assert.AreEqual(reload.Data.First(x => x.Id == update.Id).MetaData.First(x => x.Name == "NewMetaData").Value, "New");
        }

        [Test]
        public void CreateColumnMapping()
        {
            var create = this.client.GetColumnMappings(this.platform, 1, null).Data.First();
            create.Id = 0;
            create.OrganizationId = -999;
            create.DisplayName = "Unit Test : CreateColumnMapping" + DateTime.Now;
            create.UniqueName = create.DisplayName;
  

            create.MetaData = new List<ReportColumnMetaDataValue>()
            {
                new ReportColumnMetaDataValue()
                {
                    Name = "DataFormat",
                    Value = "Number"
                },
                new ReportColumnMetaDataValue()
                {
                    Name = "Precision",
                    Value = "2"
                },
                new ReportColumnMetaDataValue()
                {
                    Name = "UiBehaviour",
                    Value = "CampaignLink"
                }
            };

            var result = this.client.CreateColumnMapping(this.platform, create);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
            Assert.AreEqual(create.DisplayName, result.Data.DisplayName);  
             
        } 


        #endregion

        #region Reports

        [Test]
        public void GenerateReport()
        {
            var request = new SearchRequest()
            {
                GroupByColumn = new SelectedColumn(101), //CampaignID
                SortByColumn = new SelectedColumn(101), //CampaignID
                PageSize = 20,
                SelectedColumns = new List<SelectedColumn> { new SelectedColumn(101) } //CampaignID
            };
            var result = this.client.GenerateReport(this.platform, 1, null, request);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data); 
        }

        [Test]
        public void GetReportStatus()
        {
            var request = new SearchRequest()
            {
                GroupByColumn = new SelectedColumn(101), //CampaignID
                SortByColumn = new SelectedColumn(101), //CampaignID
                PageSize = 20,
                SelectedColumns = new List<SelectedColumn> { new SelectedColumn(101) } //CampaignID
            };
            var report = this.client.GenerateReport(this.platform, 1, null, request);

            var result = this.client.GetReportStatus(this.platform, report.Data.Id);

            Assert.NotNull(result);
            Assert.Null(result.Error);
            Assert.NotNull(result.Data);
        }

        #endregion

    }
}