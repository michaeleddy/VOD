using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VOD
{
    public sealed class HttpClientEx : IDisposable
    {
        private HttpClient HttpClient { get; }
        public HttpClientEx()
        {
            var handler = new WebRequestHandler
            {
                ServerCertificateValidationCallback = delegate { return true; }
            };
            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.Referrer = new Uri("http://www.bilibili.com/");
            HttpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 BiliDroid/4.34.0 (bbcallen@gmail.com)");
        }
        public async Task<string> PostResults(string requestUri, string sendContent)
        {
            try
            {
                HttpContent httpContent = new StringContent(sendContent);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await HttpClient.PostAsync(requestUri, httpContent);
                response.EnsureSuccessStatusCode();
                var encodeResults = await response.Content.ReadAsByteArrayAsync();
                return Encoding.UTF8.GetString(encodeResults, 0, encodeResults.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
        public async Task<string> GetStringAsync(Uri uri)
        {
            try
            {
                return await HttpClient.GetStringAsync(uri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default;
            }
        }
        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            try
            {
                return await HttpClient.GetAsync(uri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default;
            }
        }
        public async Task<string> GetResults(Uri uri)
        {
            try
            {
                HttpResponseMessage hr = await HttpClient.GetAsync(uri);
                hr.EnsureSuccessStatusCode();
                var encodeResults = await hr.Content.ReadAsByteArrayAsync();
                return Encoding.UTF8.GetString(encodeResults, 0, encodeResults.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
        public void Dispose()
        {
            try
            {
                HttpClient.Dispose();
            }
            catch { }
        }
    }
}