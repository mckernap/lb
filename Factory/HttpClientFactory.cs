namespace lb.Factory
{
    public class HttpClientFactory : IHttpClientFactory
    {
        HttpMessageHandler? _httpMessageHandler;

        public HttpClientFactory(HttpMessageHandler? httpMessageHandler = null)
        {
            _httpMessageHandler = httpMessageHandler;
        }

        public HttpClient CreateClient()
        {
            return _httpMessageHandler != null ? new HttpClient(_httpMessageHandler) : new HttpClient();
        }
    }    
}
