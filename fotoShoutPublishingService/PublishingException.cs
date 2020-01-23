using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService {
    public class PublishingException: Exception {
        public PublishingException()
            : base() {
        }

        public PublishingException(string msg)
            : base(msg) {
        }

        public PublishingException(string msg, Exception innerException)
            : base(msg, innerException) {
        }
    }
}
