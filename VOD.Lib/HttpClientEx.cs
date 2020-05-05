using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VOD.Lib.Libs;

namespace VOD
{
    public sealed class HttpClientEx : IDisposable
    {
        public HttpClient BaseHttpClient { get; }
        public HttpClientEx()
        {
            var handler = new WebRequestHandler
            {
                ServerCertificateValidationCallback = delegate { return true; }
            };
            BaseHttpClient = new HttpClient(handler);
            BaseHttpClient.DefaultRequestHeaders.Referrer = new Uri("http://www.bilibili.com/");
            BaseHttpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 BiliDroid/4.34.0 (bbcallen@gmail.com)");
        }
        public async Task<string> PostResults(string requestUri, string sendContent)
        {
            try
            {
                HttpContent httpContent = new StringContent(sendContent);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await BaseHttpClient.PostAsync(requestUri, httpContent);
                response.EnsureSuccessStatusCode();
                var encodeResults = await response.Content.ReadAsByteArrayAsync();
                return Encoding.UTF8.GetString(encodeResults, 0, encodeResults.Length);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("PostResults", ex);
                return string.Empty;
            }
        }
        public async Task<string> GetStringAsync(Uri uri)
        {
            try
            {
                return await BaseHttpClient.GetStringAsync(uri);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("GetStringAsync", ex);
                return default;
            }
        }
        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            try
            {
                return await BaseHttpClient.GetAsync(uri);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("GetAsync", ex);
                return default;
            }
        }
        public async Task<string> GetResults(Uri uri)
        {
            try
            {
                HttpResponseMessage hr = await BaseHttpClient.GetAsync(uri);
                hr.EnsureSuccessStatusCode();
                var encodeResults = await hr.Content.ReadAsByteArrayAsync();
                return Encoding.UTF8.GetString(encodeResults, 0, encodeResults.Length);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("GetResults", ex);
                return string.Empty;
            }
        }
        public void Dispose()
        {
            try
            {
                BaseHttpClient.Dispose();
            }
            catch { }
        }
    }
}