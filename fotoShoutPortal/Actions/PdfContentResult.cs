using FotoShoutUtils.Utils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FotoShoutPortal.Actions {
    public class PdfContentResult: PdfResult {
        public PdfContentResult(byte[] fileContent)
            : base() {
            if (fileContent == null) {
                throw new ArgumentNullException("Value cannot be null or empty.", "fileContent");
            }
            this.FileContent = fileContent;
        }

        public byte[] FileContent { get; private set; }

        protected override void WriteFile(HttpResponseBase response) {
            response.OutputStream.Write(this.FileContent, 0, this.FileContent.Length);
        }
    }
}