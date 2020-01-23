using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutApi.Services {
    public class DsUpdateException : Exception {
        public DsUpdateException()
            : base() {
        }

        public DsUpdateException(string message)
            : base(message) {
        }

        public DsUpdateException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
