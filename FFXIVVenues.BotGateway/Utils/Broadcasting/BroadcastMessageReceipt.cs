namespace FFXIVVenues.Veni.Utils.Broadcasting;

public class BroadcastMessageReceipt
{
    
    public ulong UserId { get; init; }
    public ulong ChannelId { get; init; }
    public ulong MessageId { get; init; }
    public MessageStatus Status { get; init; }
    public string Log { get; init; }
    
    public BroadcastMessageReceipt() { }

    public BroadcastMessageReceipt(BroadcastMessage m)
        : this(m.UserId, m.Message?.Channel?.Id ?? 0, m.Message?.Id ?? 0, m.Status, m.Log)
    { }

    public BroadcastMessageReceipt(ulong userId, ulong channelId, ulong messageId, MessageStatus status, string log)
    {
        this.UserId = userId;
        this.ChannelId = channelId;
        this.MessageId = messageId;
        this.Status = status;
        this.Log = log;
    }

};