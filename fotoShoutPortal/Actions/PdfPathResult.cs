using FotoShoutUtils.Utils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FotoShoutPortal.Actions {
    public class PdfPathResult: PdfResult {
        public PdfPathResult(string filename)
            : base() {
            if (string.IsNullOrEmpty(filename)) {
                throw new ArgumentNullException("Value cannot be null or empty.", "filename");
            }
            this.Filename = filename;
        }

        public string Filename { get; private set; }

        protected override void WriteFile(HttpResponseBase response) {
            HttpContext context = HttpContext.Current;
            response.TransmitFile(this.Filename);
        }
    }
}