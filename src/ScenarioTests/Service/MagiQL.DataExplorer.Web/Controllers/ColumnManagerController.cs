using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using MagiQL.Framework.Model.Columns;
using MagiQL.Service.Interfaces;
using MagiQL.Service.Client;
using Newtonsoft.Json;

namespace MagiQL.DataExplorer.Web.Controllers
{
    public class ColumnManagerController : Controller
    {
        private readonly IReportsService _reportsService;
        
        public ColumnManagerController()
        {
            _reportsService = new ReportsServiceClient();
        }
         
        public ActionResult Index(string platform, int orgId = 0)
        {
            ViewBag.Platform = platform;
            ViewBag.OrganizationId = orgId;

            var model = _reportsService.GetColumnMappings(platform, orgId, null);

            return View(model);
        }


        [HttpGet]
        public ActionResult Create(string platform, int orgId = 0)
        {
            var model = new ReportColumnMapping
            {
                DataSourceTypeId = 1,  
                OrganizationId = orgId > 0 ? orgId :(int?)null,
                FieldAggregationMethod = FieldAggregationMethod.Sum,
                DbType = DbType.Int32,
                KnownTable = "Stats",
                MetaData = new List<ReportColumnMetaDataValue>()
                {
                    new ReportColumnMetaDataValue(){Name = "DataFormat", Value = "Number"}
                }
            };

            return View("Edit", model);
        }



        [HttpPost]
        public ActionResult Create(ColumnUpdatePostModel formModel, string platform)
        {
            var model = new ReportColumnMapping
            {
                DataSourceTypeId = 1, 
                MetaData = new List<ReportColumnMetaDataValue>()
            };

            SetValues(formModel, model);

            Framework.Model.Response.CreateColumnMappingResponse result = null;
            bool hasError = false;
            try
            {
                result = _reportsService.CreateColumnMapping(platform, model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Create Failed \n\n" + ex.Message;
                hasError = true;
            }

            if (!hasError && result != null && result.Error != null && result.Error.Message != null)
            {
                ViewBag.ErrorMessage = "Create Failed \n\n" + result.Error.Message;
                hasError = true;
            }

            if (hasError)
            {
                return View("Edit", model);
            }


            return RedirectToAction("Index", new { platform = platform });
        }


        [HttpGet]
        public ActionResult Edit(int id, string platform, int orgId = 0)
        {
            var allMappings = _reportsService.GetColumnMappings(platform, orgId, null, columnId: id);

            var model = allMappings.Data.First();

            return View(model);

        }

        [HttpPost]
        public ActionResult Edit(int id, ColumnUpdatePostModel formModel, string platform)
        {
            if (formModel.Id != id)
            {
                throw new Exception("Id Mismatch");
            }

            var orgId = formModel.OrganizationId ?? 0;

            var allMappings = _reportsService.GetColumnMappings(platform, orgId, null, columnId: id);
            var existing = allMappings.Data.Single();
             
            SetValues(formModel, existing);
            
            Framework.Model.Response.UpdateColumnMappingResponse result = null;
            bool hasError = false;
            try
            {
                result = _reportsService.UpdateColumnMapping(platform, id, null, existing);
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = "Update Failed \n\n" + ex.Message;
                hasError = true;
            }

            if (!hasError && result != null && result.Error != null && result.Error.Message != null)
            {
                ViewBag.ErrorMessage = "Update Failed \n\n" + result.Error.Message;
                hasError = true;
            }
            
            if (hasError)
            {
                return View(existing);
            }


            return RedirectToAction("Index", new {platform = platform, orgId = orgId});
        }

        private static void SetValues(ColumnUpdatePostModel formModel, ReportColumnMapping existing)
        {
            existing._DbTypeString = formModel._DbTypeString;
            existing._FieldAggregationMethodString = formModel._FieldAggregationMethodString;
            existing.ActionSpecId = formModel.ActionSpecId;
            existing.CanGroupBy = formModel.CanGroupBy;
            existing.CreatedByUserId = formModel.CreatedByUserId;
            existing.DisplayName = formModel.DisplayName;
            existing.FieldName = formModel.FieldName;
            existing.IsCalculated = formModel.IsCalculated;
            existing.IsPrivate = formModel.IsPrivate;
            existing.IsStat = formModel.IsStat;
            existing.KnownTable = formModel.KnownTable;
            existing.Selectable = formModel.Selectable;
            existing.MainCategory = formModel.MainCategory;
            existing.SubCategory = formModel.SubCategory;
            existing.UniqueName = formModel.UniqueName;
            existing.OrganizationId = formModel.OrganizationId;

            existing.MetaData = JsonConvert.DeserializeObject<List<ReportColumnMetaDataValue>>(formModel.MetaData);
        }


        public ActionResult Schema(string platform)
        {
            var model = _reportsService.GetTableInfo(platform);
            return View("Schema", model.Data);
        }

        public ActionResult DependantColumns(int id,string platform)
        {
            var result = _reportsService.GetDependantColumns(platform, id);

            var model = result.Data;

            return View(model);
        }


        public ActionResult DependantColumnsByFieldName(string fieldName, string platform)
        {
            var result = _reportsService.GetDependantColumns(platform, Server.UrlDecode(fieldName));

            var model = result.Data;

            return View("DependantColumns",model);
        }
    }
}
