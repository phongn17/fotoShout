using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutUtils.Service {
    public class WebServiceException : Exception {
        public WebServiceException()
            : base() {
        }
        
        public WebServiceException(string msg)
            : base(msg) {
        }
        
        public WebServiceException(string msg, Exception innerException)
            : base(msg, innerException) {
        }
    }
}
