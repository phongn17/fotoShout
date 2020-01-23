using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Services {
    public class InvalidArgumentException: Exception {
        public InvalidArgumentException()
            : base() {
        }

        public InvalidArgumentException(string message)
            : base(message) {
        }

        public InvalidArgumentException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}