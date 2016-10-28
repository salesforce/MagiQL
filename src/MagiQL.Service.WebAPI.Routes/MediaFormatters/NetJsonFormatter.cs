/*
using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NetJSON;

namespace MagiQL.Service.WebAPI.Routes.MediaFormatters
{
    /// <summary>
    /// A custom JSON MediaTypeFormatter that uses JSON.NET for deserialization and 
    /// NetJSON (https://github.com/rpgmaker/NetJSON) for serialization. 
    /// 
    /// NetJSON is supposed to be faster than JSON.NET. This MediaTypeFormatter could 
    /// therefore be used to serialize very large responses more efficiently.
    /// 
    /// At the time of writing, this MediaTypeFormatter isn't being used (don't know why).
    /// </summary>
    public class NetJsonFormatter : MediaTypeFormatter
    {
        private readonly NetJSON.NetJSONSettings _options;

        public NetJsonFormatter()
        { 
            _options = new NetJSONSettings()
            {
                DateFormat = NetJSONDateFormat.JsonNetISO,
                TimeZoneFormat = NetJSONTimeZoneFormat.Local,
                Format = NetJSONFormat.Default,
                SkipDefaultValue = true,
            };

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.FromResult(DeserializeFromStream(type, readStream, SelectCharacterEncoding(content.Headers)));
        }

        private object DeserializeFromStream(Type type, Stream readStream, Encoding encoding)
        {
            try
            {
                using (var reader = new StreamReader(readStream, encoding))
                {
                    // We'll use json.net to deserialize just to be more tolerant of requests
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd(), type);
                }
            }
            catch
            {
                return null;
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                NetJSON.NetJSON.Serialize(value, streamWriter, _options);
                return Task.FromResult(writeStream);
            }
        }
    }
}
*/