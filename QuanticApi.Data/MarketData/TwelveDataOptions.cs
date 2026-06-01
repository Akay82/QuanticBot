namespace QuanticApi.Data.MarketData;

public sealed class TwelveDataOptions
{
    public const string SectionName = "TwelveData";
    public const string ExchangeName = "TWELVE_DATA";

    public string ApiKey { get; set; } = string.Empty;
    public bool EnableWorker { get; set; } = true;
    public int RefreshIntervalSeconds { get; set; } = 600;
    public int InitialOutputSize { get; set; } = 240;
    public int IncrementalOutputSize { get; set; } = 20;
    public string Interval { get; set; } = "1min";
    public string StoredTimeframe { get; set; } = "1m";
    public List<ForexPairOptions> ForexPairs { get; set; } = [];
}

public sealed class ForexPairOptions
{
    public string Symbol { get; set; } = string.Empty;
    public string ProviderSymbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string QuoteCurrency { get; set; } = string.Empty;
}
