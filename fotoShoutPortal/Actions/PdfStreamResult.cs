using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FotoShoutPortal.Actions {
    public class PdfStreamResult: PdfResult {
        private const int BUFFER_SIZE = 0x1000;

        public PdfStreamResult(Stream fs)
            : base() {
            if (fs == null) {
                throw new ArgumentNullException("fileStream");
            }
            this.FileStream = fs;
        }

        public Stream FileStream { get; private set; }

        protected override void WriteFile(HttpResponseBase response) {
            Stream os = response.OutputStream;
            byte[] buffer = new byte[BUFFER_SIZE];
            while (true) {
                int count = this.FileStream.Read(buffer, 0, BUFFER_SIZE);
                if (count == 0)
                    return;
                os.Write(buffer, 0, count);
            }
        }
    }
}