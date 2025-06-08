// UrlScanner_HttpResponse_Accessors.cs
// Demonstrates safe accessor patterns for HttpResponseMessage in UrlScanner

using System.Net.Http;

namespace UrlScannerLib
{
    public partial class UrlScanner
    {
        // Returns the status code of the response, or null if no response
        public System.Net.HttpStatusCode? GetStatusCode() => _response?.StatusCode;

        // Returns the reason phrase of the response, or null if no response
        public string? GetReasonPhrase() => _response?.ReasonPhrase;

        // Returns true if the response indicates success, false otherwise
        public bool IsSuccessStatusCode() => _response?.IsSuccessStatusCode ?? false;

        // Returns the HTTP version
        public Version? GetVersion() => _response?.Version;

        // Returns the response content as a byte array (async)
        public async Task<byte[]?> GetContentAsByteArrayAsync() => _response != null ? await _response.Content.ReadAsByteArrayAsync() : null;

        // Returns the response content as a string (async)
        public async Task<string?> GetContentAsStringAsync() => _response != null ? await _response.Content.ReadAsStringAsync() : null;
    }
}
