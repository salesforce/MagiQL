using System.IO;
using System.Net.Mime;
using System.Web.Mvc;
using MagiQL.Service.Interfaces;
using MagiQL.Service.Client;

namespace MagiQL.DataExplorer.Web.Controllers
{
    public class ExportStatusController : Controller
    {
        private readonly IReportsService _reportsService;

        public ExportStatusController()
        {
            _reportsService = new ReportsServiceClient();
        }
          
        public ActionResult Index(int id, string platform = "facebook", int orgId = 1)
        {
            ViewBag.Platform = platform;
            ViewBag.OrganizationId = orgId;
            var result = _reportsService.GetReportStatus(platform, id, Configuration.UserId);
            return View(result.Data);
        } 

        public ActionResult Download(int id, string platform = "facebook")
        { 
            var status = _reportsService.GetReportStatus(platform, id, Configuration.UserId);
            var fullName = status.FilePath;
            var fileName = status.FileName;

            byte[] fileBytes = GetFile(fullName);
            return File(fileBytes, MediaTypeNames.Application.Octet, fileName); 
             
        }

        byte[] GetFile(string s)
        {
            FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new IOException(s);
            return data;
        }

    }
}
