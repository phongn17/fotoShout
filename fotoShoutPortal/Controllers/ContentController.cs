using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using FotoShoutUtils.Utils.IO;
using FotoShoutPortal.Actions;
using System.IO;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class ContentController : AuthenticatedController {
        //
        // GET: /Content/Download?filename=<filename>
        public ActionResult Download(string filename) {
            string mp = Server.MapPath("~/Views/Content/" + filename);
            string contentType = FileUtils.GetContentType(filename);
            return File(mp, contentType, filename);
        }

        //
        // GET: /Content/Help
        public ActionResult Help() {
            return PartialView("_Help");
        }

        //
        // GET: /Content/Pdf?filename=<filename>
        public ActionResult Pdf(string filename) {
            string mp = Server.MapPath("~/Views/Content/Help/" + filename);
            byte[] pdfContent = System.IO.File.ReadAllBytes(mp);
            return new PdfContentResult(pdfContent);
        }
    }
}
