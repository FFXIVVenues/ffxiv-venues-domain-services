using FFXIVVenues.DomainSecurity;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FFXIVVenues.WebHookService;

public class WebHookEventDistributor
{
    private readonly List<WebHookDispatcher> brokers;
    private readonly Signer signer;

    public WebHookEventDistributor(IHttpClientFactory http, IEnumerable<WebHook> webHookConfigs, Signer signer)
    {
        this.signer = signer;
        this.brokers = webHookConfigs.Select(c => new WebHookDispatcher(c, http)).ToList();
    }

    public WebHookEvent<T> DispatchEvent<T>(string eventType, T eventData)
    {
        var @event = new WebHookEvent<T>(eventType, eventData);

        var id = @event.Id;
        var timestamp = @event.EventTime.ToUnixTimeSeconds();
        var body = JsonSerializer.Serialize(@event, JsonSerializerOptions.Web);
        var signature = this.signer.Sign($"{id}.{timestamp}.{body}");
        var payload = new WebHookEventPayload(id, timestamp, body, signature);

        foreach (var broker in this.brokers)
            broker.Enqueue(eventType, payload);

        return @event;
    }
}
