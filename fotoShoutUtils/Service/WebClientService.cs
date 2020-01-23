using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Net;

namespace FotoShoutUtils.Service {
    public class WebClientService {
        public string BaseAddress { get; set; }
        public string Prefix { get; set; }
        public string MediaType { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public WebClientService() {
        }

        public WebClientService(string baseAddress) {
            BaseAddress = baseAddress;
        }

        public WebClientService(string baseAddress, string prefix) 
            : this(baseAddress) {
            Prefix = prefix;
        }

        public WebClientService(string baseAddress, string prefix, string mediaType) 
            : this(baseAddress, prefix) {
            MediaType = mediaType;
        }

        public IEnumerable<T> GetList<T>(string address, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.GetAsync(Prefix + "/" + address).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<IEnumerable<T>>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public T Get<T>(string address, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.GetAsync(Prefix + "/" + address).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<T>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public HttpResponseMessage Put(string address, HttpContent content, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, false);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.PutAsync(Prefix + "/" + address, content).Result;
                    if (response.IsSuccessStatusCode) {
                        return response;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public T Put<T>(string address, HttpContent content, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, false);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.PutAsync(Prefix + "/" + address, content).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<T>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public HttpResponseMessage Post(string address, HttpContent content, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, false);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.PostAsync(Prefix + "/" + address, content).Result;
                    if (response.IsSuccessStatusCode) {
                        return response;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public T Post<T>(string address, HttpContent content, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, true);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.PostAsync(Prefix + "/" + address, content).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<T>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public T PostFile<T>(string address, HttpContent content, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, false);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.PostAsync(address, content).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<T>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public T Delete<T>(string address, bool async) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (HttpClient client = new HttpClient()) {
                // Prepare HTTP headers for the request
                PrepareRequest(client, false);

                // Calling to the API
                if (async) {
                    HttpResponseMessage response = client.DeleteAsync(Prefix + "/" + address).Result;
                    if (response.IsSuccessStatusCode) {
                        return response.Content.ReadAsAsync<T>().Result;
                    }
                    else {
                        throw new HttpClientServiceException(response.StatusCode, response.ReasonPhrase);
                    }
                }
                else
                    throw new NotImplementedException("Has not implemented sync request");
            }
        }

        public string UploadString(string address, string str, string method = null) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (WebClient client = new WebClient()) {
                PrepareRequest(client);

                try {
                    return (string.IsNullOrEmpty(method) ? client.UploadString(BaseAddress + Prefix + "/" + address, str) : client.UploadString(BaseAddress + Prefix + "/" + address, method, str));
                }
                catch (Exception ex) {
                    throw new HttpClientServiceException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public string UploadFile(string address, string filename) {
            if (string.IsNullOrEmpty(BaseAddress) || string.IsNullOrEmpty(Prefix))
                throw new NullReferenceException("Need to provide base address, and prefix prior to this call");

            using (WebClient client = new WebClient()) {
                byte[] bytes = client.UploadFile(BaseAddress + address, filename);
                return Encoding.Default.GetString(bytes);
            }
        }

        private void PrepareRequest(WebClient client) {
            if (Headers != null) {
                foreach (KeyValuePair<string, string> entry in Headers)
                    client.Headers.Add(entry.Key, entry.Value);
            }
            if (MediaType != null)
                client.Headers[HttpRequestHeader.ContentType] = MediaType;
        }
        
        private void PrepareRequest(HttpClient client, bool output = true) {
            client.BaseAddress = new Uri(BaseAddress);

            if (Headers != null) {
                foreach (KeyValuePair<string, string> entry in Headers)
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }

            // Add an Accept header for the media type
            if (output && (MediaType != null))
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
        }
    }
}
