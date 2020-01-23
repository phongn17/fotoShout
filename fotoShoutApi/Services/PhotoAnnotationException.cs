using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Services {
    public class PhotoAnnotationException: Exception {
        public PhotoAnnotationException()
            : base() {
        }

        public PhotoAnnotationException(string message)
            : base(message) {
        }

        public PhotoAnnotationException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}