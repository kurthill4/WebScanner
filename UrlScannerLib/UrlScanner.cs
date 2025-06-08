using HtmlAgilityPack;



namespace UrlScannerLib;

public partial class UrlScanner
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly Uri _uri;
    private readonly string _baseaddress;
    private HttpResponseMessage? _response;
    private string? _content;

    // Error tracking properties
    public bool IsSuccess { get; private set; } = true;
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }

    private UrlScanner(Uri uri)
    {
        _uri = uri;
        _baseaddress = uri.GetLeftPart(UriPartial.Authority);
    }

    // CreateAsync: Asynchronously creates a UrlScanner instance for the specified URL.
    // This method validates the URL, ensuring it is absolute and uses HTTP or HTTPS.
    // Once it has a verified URL converted to a URI, it calls the other CreateAsync method.

    public static async Task<UrlScanner> CreateAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or whitespace.", nameof(url));
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new UriFormatException($"Invalid URL: {url}");

        // Check to ensure the protocol is HTTP or HTTPS
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new NotSupportedException($"Unsupported URL scheme: {uri.Scheme}. Only HTTP and HTTPS are supported.");
        }

        UrlScanner scanner = await UrlScanner.CreateAsync(uri);
        return scanner;
    }

    // CreateAsync: Asynchronously creates a UrlScanner instance for the specified URI.
    public static async Task<UrlScanner> CreateAsync(Uri uri)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri), "URI cannot be null.");

        // Check to ensure the protocol is HTTP or HTTPS
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new NotSupportedException($"Unsupported URL scheme: {uri.Scheme}. Only HTTP and HTTPS are supported.");
        }

        // Load the page asynchronously
        HttpResponseMessage? response = null;
        string? content = null;
        Exception? exception = null;
        string? errorMessage = null;
        bool isSuccess = false;
        try
        {
            response = await LoadPageAsync(uri);
            content = await response.Content.ReadAsStringAsync();
            isSuccess = response.IsSuccessStatusCode;
            if (!isSuccess)
            {
                errorMessage = $"HTTP error: {(int)response.StatusCode} {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            exception = ex;
            errorMessage = ex.Message;
        }

        var scanner = new UrlScanner(uri)
        {
            _response = response,
            _content = content,
        };
        scanner.IsSuccess = isSuccess;
        scanner.ErrorMessage = errorMessage;
        scanner.Exception = exception;
        return scanner;
    }

    // LoadPage: Asynchronously loads the page at the specified URL.
    // Do not throw an exception for failed loads, just return the response (or null if network error).
    public static async Task<HttpResponseMessage> LoadPageAsync(Uri uri)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri), "URI cannot be null.");
        // Let exceptions propagate to the caller for proper error reporting
        return await _httpClient.GetAsync(uri);
    }

    public string? GetContent()
    {
        return _content;
    }
    
public List<string> ExtractHyperlinks()
    {
        var links = new List<string>();
        if (string.IsNullOrEmpty(_content))
            return links;

        var doc = new HtmlDocument();
        doc.LoadHtml(_content);

        HtmlNodeCollection? nodes = doc.DocumentNode.SelectNodes("//a[@href]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                string href = node.GetAttributeValue("href", String.Empty);
                if (!string.IsNullOrEmpty(href))
                    links.Add(href);
            }
        }

        return links;
    }

    public static void SetTimeout(int seconds)
    {
        if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds), "Timeout must be positive.");
        _httpClient.Timeout = TimeSpan.FromSeconds(seconds);
    }

}
