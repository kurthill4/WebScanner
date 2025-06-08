using HtmlAgilityPack;



namespace UrlScannerLib;

public partial class UrlScanner
{
    private readonly Uri _uri;
    private readonly string _baseaddress;
    private HttpResponseMessage? _response;
    private string? _content;

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
        HttpResponseMessage response = await LoadPageAsync(uri);
        UrlScanner scanner = new UrlScanner(uri)
        {
            _response = response,
            _content = await response.Content.ReadAsStringAsync()
        };

        return scanner;
    }
    // LoadPage: Asynchronously loads the page at the specified URL.
    // Do not throw an exception for failed loads, just return the response.
    // This allows the caller to determine if this is a broken link.
    // This is a static function as it may be useful as a utility function.
    public static async Task<HttpResponseMessage> LoadPageAsync(Uri uri)
    {
        if (uri == null)
            throw new ArgumentNullException(nameof(uri), "URI cannot be null.");

        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = await httpClient.GetAsync(uri);
        return response;
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

        foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlNodeCollection(null))
        {
            var href = link.GetAttributeValue("href", null);
            if (!string.IsNullOrEmpty(href))
                links.Add(href);
        }
        return links;
    }

}
