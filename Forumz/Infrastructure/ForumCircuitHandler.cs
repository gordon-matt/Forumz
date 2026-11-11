using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Forumz.Infrastructure;

/// <summary>
/// Remembers the remote address when the circuit opens. Later posts run over SignalR, where
/// <c>HttpContext</c> is gone.
/// </summary>
internal sealed class ForumCircuitHandler(ForumClientContext client, IHttpContextAccessor http) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        client.IpAddress = http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        return Task.CompletedTask;
    }
}
