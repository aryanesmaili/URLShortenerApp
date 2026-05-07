using System.Text.Json.Serialization;
using URLShortener.Domain.Entities.URL;

namespace URLShortener.Application.Common.Models;

public class IncomingRequestMetadata
{
    public required string IPAddress { get; set; }
    public required string UserAgent { get; set; }
    public required DateTime TimeClicked { get; set; }
    [JsonInclude]
    public URLModel? URL { get; set; }
}
