using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MagiQL.DataExplorer.Web.Controllers
{
    public class ImpersonateController : Controller
    {
        //
        // GET: /Impersonate/

        public ActionResult Index(string platform, int organizationId, int userId)
        {
            Response.Cookies.Set(new HttpCookie("Impersonate_OrganizationId") { Value = organizationId.ToString() });
            Response.Cookies.Set(new HttpCookie("Impersonate_UserId") { Value = userId.ToString() });

            return Redirect(Request.UrlReferrer?.AbsoluteUri ?? "/");
        }

        public ActionResult Form()
        {
            ViewBag.OrganizationId = GetOrgId(Request);
            ViewBag.UserId = GetUserId(Request);

            return View();
        }

        public static int GetUserId(HttpRequestBase request)
        {
            return int.Parse(request.Cookies["Impersonate_UserId"]?.Value ?? "1");
        }


        public static int GetOrgId(HttpRequestBase request)
        {
            return int.Parse(request.Cookies["Impersonate_OrganizationId"]?.Value ?? "1");
        }
    }
}
