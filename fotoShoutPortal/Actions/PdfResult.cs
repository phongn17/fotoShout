using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Actions {
    public abstract class PdfResult: FileResult {
        public PdfResult()
            : base(System.Net.Mime.MediaTypeNames.Application.Pdf) {
        }
    }
}