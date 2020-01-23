using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public class TransferMechanism {
        private System.IO.Stream _dataStream;
        private string _uri;

        public TransferMechanism(System.IO.Stream dataStream) {
            _dataStream = dataStream;
        }

        public TransferMechanism(System.IO.Stream dataStream, string uri) {
            _dataStream = dataStream;
            _uri = uri;
        }

        public System.IO.Stream DataStream {
            get { return _dataStream; }
        }

        public string Uri {
            get { return _uri; }
        }
    }
}
