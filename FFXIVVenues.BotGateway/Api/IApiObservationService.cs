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


namespace FFXIVVenues.Veni.Api
{
    public interface IApiObservationService
    {
        public Task ObserveAsync();
        public IApiObservationService Observe<T>(params ObservableOperation[] operations) where T : IApiObserver;
    }

}