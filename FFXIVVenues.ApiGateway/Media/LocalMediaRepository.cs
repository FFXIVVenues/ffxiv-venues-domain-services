using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.DomainData.Helpers;
using Microsoft.Extensions.Configuration;

namespace FFXIVVenues.ApiGateway.Media;

public class LocalMediaRepository : IMediaRepository
{
    private readonly IConfiguration _config;

    private const string DEFAULT_MEDIA_LOCATION = "media/";
    private const string TYPES_FILE = "contentTypes";

    private readonly string _mediaLocation;
    private readonly string _typesFile;
    private readonly Dictionary<string, string> _contentTypes;

    public bool IsMetered => false;

    public LocalMediaRepository(IConfiguration config)
    {
        this._config = config;
        this._mediaLocation = _config.GetValue("MediaStorage:Local:FilePath", DEFAULT_MEDIA_LOCATION);
        Directory.CreateDirectory(this._mediaLocation);
        this._typesFile = this._mediaLocation + TYPES_FILE;
        if (File.Exists(this._typesFile))
            this._contentTypes = File.ReadAllLines(this._typesFile).ToDictionary(l => l.Split("::")[0], l => l.Split("::")[1]);
        else
            this._contentTypes = new ();
    }

    public Task Delete(string venueId, string key)
    {
        this._contentTypes.Remove(key);
        var dir = Path.Combine(this._mediaLocation, venueId);
        var path = Path.Combine(dir, key);
        File.Delete(Path.Combine(path));
        if (new DirectoryInfo(dir).GetFiles().Length == 0)
            Directory.Delete(dir);
        return this.WriteTypesFileAsync();
    }

    public Task<(Stream Stream, string ContentType)> Download(string venueId, string key, CancellationToken _)
    {
        if (!this._contentTypes.ContainsKey(key))
            return Task.FromResult((Stream.Null, ""));
        
        var path = Path.Combine(this._mediaLocation, venueId, key);
        if (!File.Exists(path))
            return Task.FromResult((Stream.Null, ""));
        
        return Task.FromResult((File.OpenRead(path) as Stream, this._contentTypes[key]));
    }

    public async Task<string> Upload(string venueId, string contentType, long contentLength, Stream stream, CancellationToken cancellationToken)
    {
        var key = IdHelper.GenerateId();
        var dir = Path.Combine(this._mediaLocation, venueId);
        var path = Path.Combine(dir, key);
        Directory.CreateDirectory(dir);
        this._contentTypes[key] = contentType;
        await using (var fileStream = File.OpenWrite(path))
            await stream.CopyToAsync(fileStream, cancellationToken);
        _ = this.WriteTypesFileAsync();
        return key;
    }

    private Task WriteTypesFileAsync()
    {
        var serialization = this._contentTypes.Select(kv => $"{kv.Key}::{kv.Value}");
        return File.WriteAllLinesAsync(this._typesFile, serialization);
    }

}