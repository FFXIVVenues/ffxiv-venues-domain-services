using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.VenueModels.Observability;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FFXIVVenues.Veni.Api
{
    internal class ApiObservationService : IApiObservationService, IDisposable
    {
        private readonly ObserveRequest _observeRequest;
        private readonly IServiceProvider _serviceProvider;
        private readonly Uri _wsUri;
        private readonly List<(ObserveRequest Request, Type ObserverType)> _observers;
        private ClientWebSocket _wsClient;
        private Task _task;

        public ApiObservationService(ApiConfiguration config, IServiceProvider serviceProvider)
        {
            this._observeRequest = new (new ObservableOperation[] {  }, null, null);
            this._observers = new ();
            this._wsUri = new Uri(config.BaseUrl.Replace("http", "ws") + "/v1.0/venue/observe");
            this._serviceProvider = serviceProvider;
        }

        public Task ObserveAsync() =>
            this._task ??= new TaskFactory().StartNew(Handle, TaskCreationOptions.LongRunning);
        
        public IApiObservationService Observe<T>(params ObservableOperation[] operations) where T : IApiObserver
        {
            this._observeRequest.OperationCriteria = this._observeRequest.OperationCriteria.Union(operations);
            _ = this.SendObservationRequestAsync();
            var @type = typeof(T);
            Log.Debug("Observer {Observer} is observing.", @type.Name);
            this._observers.Add((new(operations, null, null), @type));
            return this;
        }
        
        private async Task Handle()
        {
            while (true)
            {
                try
                {
                    await ConnectToEndpoint();
                    _ = SendObservationRequestAsync();
                    await ListenToEndpoint();
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task ListenToEndpoint()
        {
            Log.Debug("Listening for observations from observe endpoint.");
            var buffer = new byte[256];
            while (this._wsClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await this._wsClient.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    Log.Warning("Lost connection to observe endpoint.");
                    return;
                }
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Debug("Connection to observe endpoint closed by remote.");
                    await this._wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    continue;
                }

                var message = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, result.Count));
                var observation = JsonSerializer.Deserialize<Observation>(message);
                if (observation == null)
                    continue;

                Log.Debug("Received observation from observe endpoint; {Observation}.", observation);
                _ = HandleObservation(observation);
            }
        }

        private async Task ConnectToEndpoint()
        {
            try
            {
                Log.Debug("Connecting to API observe ws endpoint.");
                this._wsClient = new();
                await this._wsClient.ConnectAsync(this._wsUri, CancellationToken.None);
                Log.Information("Connected to API observe ws endpoint.");
            }
            catch
            {
                Log.Warning("Failed to connect to API observe ws endpoint.");
                throw;
            }
        }

        private Task SendObservationRequestAsync()
        {
            if (this._wsClient?.State != WebSocketState.Open)
                return Task.CompletedTask;
            Log.Debug("Sending observation request to observe endpoint.");
            var requestMessage = JsonSerializer.Serialize(this._observeRequest);
            return this._wsClient.SendAsync(Encoding.ASCII.GetBytes(requestMessage),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task HandleObservation(Observation observation)
        {
            foreach (var observer in this._observers)
            {
                if (!observer.Request.OperationCriteria.Contains(observation.Operation)) continue;
                var observerInstance = ActivatorUtilities.CreateInstance(this._serviceProvider, observer.ObserverType) as IApiObserver;
                if (observerInstance == null) continue;
                Log.Debug("Observer {Observer} handling observation.", observer.ObserverType.Name);
                await observerInstance.Handle(observation);
            }
        }
        
        public void Dispose()
        {
            _wsClient?
                .CloseAsync(WebSocketCloseStatus.NormalClosure, "Observation ended", CancellationToken.None)
                .ContinueWith(_ => _wsClient?.Dispose());
            _task?.Dispose();
        }

    }

}