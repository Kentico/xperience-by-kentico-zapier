using System.Net;

namespace Kentico.Xperience.Zapier.Tests.Support;

/// <summary>
/// Captures outgoing webhook requests and answers with a configurable status code.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode;
    private readonly string responseBody;


    /// <summary>When set, every request throws this exception instead of returning a response.</summary>
    public Exception? ThrowOnSend { get; set; }


    private readonly List<(HttpRequestMessage Request, string Body)> requests = [];


    /// <summary>Snapshot of captured requests; deliveries arrive from thread-pool threads.</summary>
    public IReadOnlyList<(HttpRequestMessage Request, string Body)> Requests
    {
        get
        {
            lock (requests)
            {
                return [.. requests];
            }
        }
    }


    public StubHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, string responseBody = "")
    {
        this.statusCode = statusCode;
        this.responseBody = responseBody;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        lock (requests)
        {
            requests.Add((request, body));
        }

        if (ThrowOnSend is not null)
        {
            throw ThrowOnSend;
        }

        return new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) };
    }
}
