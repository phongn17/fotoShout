using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Service;
using FotoShoutUtils.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    public class PhotoWebsiteController : Controller {
        readonly static ILog _logger = LogManager.GetLogger(typeof(PhotoWebsiteController));
        //
        // GET: /EventWebsite/
        public ActionResult Index(string photo, string hi = null, string hu= null, string ti = null, string tu = null, string bi = null, string bu = null, string fi = null, string fu = null) {
            var model = GeneratePhotoWebsite(photo, hi, hu, ti, tu, bi, bu, fi, fu);
            return View(model);
        }

        private PhotoWebsiteTDO GeneratePhotoWebsite(string photo, string hi = null, string hu = null, string ti = null, string tu = null, string bi = null, string bu = null, string fi = null, string fu = null) {
            return new PhotoWebsiteTDO {
                PhotoUrl = photo,
                HeaderImage = HttpUtility.UrlDecode(hi),
                HeaderUrl = HttpUtility.UrlDecode(hu),
                TopInfoBlockImage = HttpUtility.UrlDecode(ti),
                TopInfoBlockUrl = HttpUtility.UrlDecode(tu),
                BottomInfoBlockImage = HttpUtility.UrlDecode(bi),
                BottomInfoBlockUrl = HttpUtility.UrlDecode(bu),
                FooterImage = HttpUtility.UrlDecode(fi),
                FooterUrl = HttpUtility.UrlDecode(fu)
            };
        }
    }
}
