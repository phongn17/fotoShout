using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FotoShoutUtils.Service {
    public class HttpClientServiceException : Exception {
        public HttpStatusCode StatusCode { get; set; }

        public HttpClientServiceException()
            : base() {
        }

        public HttpClientServiceException(HttpStatusCode statusCode, string message)
            : base(message) {
            StatusCode = statusCode;
        }

        public HttpClientServiceException(HttpStatusCode statusCode, string message, Exception innerException)
            : base(message, innerException) {
            StatusCode = statusCode;
        }
    }
}
