using System.Collections.Generic;
using System.Linq;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.BotGateway.Utils.Broadcasting;

public class BroadcastReceipt : IEntity
{
    public string id { get; set; }
    public List<BroadcastMessageReceipt> BroadcastMessages { get; set; }
    
    public BroadcastReceipt() { }

    public BroadcastReceipt(string id, IEnumerable<BroadcastMessage> messages)
    {
        this.id = id;
        this.BroadcastMessages = messages.Select(m => new BroadcastMessageReceipt(m))
            .ToList();
    }
}