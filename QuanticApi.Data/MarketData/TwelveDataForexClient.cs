using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace QuanticApi.Data.MarketData;

public sealed class TwelveDataForexClient(
    HttpClient httpClient,
    IOptions<TwelveDataOptions> options)
{
    public async Task<TwelveDataTimeSeriesResponse> GetTimeSeriesAsync(
        string providerSymbol,
        string interval,
        int outputSize,
        CancellationToken cancellationToken)
    {
        var apiKey = options.Value.ApiKey;
        var uri =
            $"time_series?symbol={Uri.EscapeDataString(providerSymbol)}" +
            $"&interval={Uri.EscapeDataString(interval)}" +
            $"&outputsize={outputSize}" +
            $"&timezone=UTC&apikey={Uri.EscapeDataString(apiKey)}";

        using var response = await httpClient.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Twelve Data returned {(int)response.StatusCode} ({response.StatusCode}). {error}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<TwelveDataTimeSeriesResponse>(cancellationToken)
            ?? new TwelveDataTimeSeriesResponse();

        if (string.Equals(result.Status, "error", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Twelve Data returned an error. {result.Message}");
        }

        return result;
    }
}

public sealed class TwelveDataTimeSeriesResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<TwelveDataCandle> Values { get; set; } = [];
}

public sealed class TwelveDataCandle
{
    [JsonPropertyName("datetime")]
    public string Datetime { get; set; } = string.Empty;

    [JsonPropertyName("open")]
    public string Open { get; set; } = string.Empty;

    [JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("close")]
    public string Close { get; set; } = string.Empty;

    [JsonPropertyName("volume")]
    public string? Volume { get; set; }
}
