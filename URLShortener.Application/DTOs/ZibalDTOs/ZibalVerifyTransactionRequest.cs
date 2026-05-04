using System.Text.Json.Serialization;

namespace URLShortener.Application.DTOs.ZibalDTOs;

/// <summary>
/// The Placeholder for the request you send to Zibal to verify the transaction.
/// </summary>
public record ZibalVerifyTransactionRequest
{
    private string merchant = string.Empty;

    /// <summary>
    /// provides access to the merchant. if this is a test, returns "zibal" else the merchant.
    /// </summary>
    public required string Merchant { get => IsTest ? "zibal" : merchant; set => merchant = value.Trim(); }

    /// <summary>
    /// The TrackID provided by Zibal in the previous stage.
    /// </summary>
    public required long TrackID { get; init; }

    /// <summary>
    /// Is This a Test Transaction?
    /// </summary>
    [JsonIgnore]
    public bool IsTest { get; init; } = false;
}
