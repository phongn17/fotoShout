using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Formatters {
    public class JsonNetFormatter: MediaTypeFormatter {
        private JsonSerializerSettings _serializerSettings;

        public JsonNetFormatter(JsonSerializerSettings serializerSettings = null) {
            _serializerSettings = serializerSettings ?? new JsonSerializerSettings();

            // Fill out the media type and encoding we support
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedEncodings.Add(new UTF8Encoding(false, true));
        }

        public override bool CanReadType(Type type) {
            return true;
        }

        public override bool CanWriteType(Type type) {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger) {
            // Create a serializer 
            JsonSerializer serializer = JsonSerializer.Create(_serializerSettings);

            // Create task reading the content
            return Task.Factory.StartNew(() => {
                using (StreamReader streamReader = new StreamReader(stream, SupportedEncodings[0])) {
                    using (JsonTextReader reader = new JsonTextReader(streamReader)) {
                        return serializer.Deserialize(reader, type);
                    }
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, System.Net.TransportContext transportContext) {
            // Create a serializer 
            JsonSerializer serializer = JsonSerializer.Create(_serializerSettings);

            // Create task reading the content
            return Task.Factory.StartNew(() => {
                using (StreamWriter streamWriter = new StreamWriter(stream, SupportedEncodings[0])) {
                    using (JsonTextWriter writer = new JsonTextWriter(streamWriter)) {
                        serializer.Serialize(writer, type);
                    }
                }
            });
        }
    }
}
