using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutApi.Controllers
{
    public class MediaController : Controller
    {
        //
        // GET: /Media/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upload() {
            //this field is never empty, it contains the selected filename 
            if (Request.Files.Count == 0) {
                ViewBag.ServerUri = "";
            }
            else {
                var file = Request.Files[0];
                if (file != null) {
                    byte[] buf = new byte[file.ContentLength];
                    file.InputStream.Read(buf, 0, file.ContentLength);
                    ViewBag.ServerUri = "test";
                }
                else {
                    ViewBag.ServerUri = "";
                }
            }
            return View();
        }
    }
}
