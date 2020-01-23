using FotoShoutApi.Models;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace FotoShoutApi.Formatters {
    public class ImageFormatter: MediaTypeFormatter {
        public ImageFormatter() {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpg"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/png"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/bmp"));
        }
        
        public override bool CanReadType(Type type) {
            return true;
        }

        public override bool CanWriteType(Type type) {
            return (type == typeof(Guest) || type == typeof(GuestTDO));
        }

        public override Task<object> ReadFromStreamAsync(Type type, System.IO.Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger) {
            throw new NotImplementedException();
        }

        public override Task WriteToStreamAsync(Type type, object value, System.IO.Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext) {
            var task = Task.Factory.StartNew(() => {
                if (value is Guest) {
                    var guest = value as Guest;
                    if (guest != null && guest.Signature != null) {
                        writeStream.Write(guest.Signature, 0, guest.Signature.Length);
                    }
                }
                else if (value is GuestTDO) {
                    var guest = value as GuestTDO;
                    if (guest != null && guest.Signature != null) {
                        writeStream.Write(guest.Signature, 0, guest.Signature.Length);
                    }
                }
                else {
                    throw new ArgumentException("Data type has not been supported.");
                }
            });

            return task;
        }
    }
}