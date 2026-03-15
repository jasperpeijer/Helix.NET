using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Helix.Client.Web.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    
    private readonly IJSRuntime _js;
    
    public AuthorizationMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
    
}