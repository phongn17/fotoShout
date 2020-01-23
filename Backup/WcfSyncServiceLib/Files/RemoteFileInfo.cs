using FotoShoutUtils.Sync.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService {
    [MessageContract]
    public class RemoteFileInfo : IDisposable {
        //public RemoteFileInfo(string path, long length, ItemMetadata im, Stream stream) {
        //    FolderPath = path;
        //    Length = length;
        //    Metadata = im;
        //    FileByteStream = stream;
        //}
        
        [MessageHeader(MustUnderstand = true)]
        public string FolderPath;

        [MessageHeader(MustUnderstand = true)]
        public ItemMetadata Metadata;

        [MessageHeader(MustUnderstand = true)]
        public long Length;

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream FileByteStream;

        public void Dispose() {
            // close stream when the contract instance is disposed. this ensures that stream is closed when file upload is complete, 
            // since upload procedure is handled by the client and the stream must be closed on server.
            // thanks Bhuddhike! http://blogs.thinktecture.com/buddhike/archive/2007/09/06/414936.aspx
            if (FileByteStream != null) {
                FileByteStream.Close();
                FileByteStream = null;
            }
        }
    }
}
