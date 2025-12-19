using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFXIVVenues.ApiGateway.Media;
    
public interface IMediaRepository
{
    bool IsMetered { get; }
    Task Delete(string venueId, string key);
    Task<(Stream Stream, string ContentType)> Download(string venueId, string key, CancellationToken cancellationToken);
    Task<string> Upload(string venueId, string contentType, long contentLength, Stream stream, CancellationToken cancellationToken);
}
