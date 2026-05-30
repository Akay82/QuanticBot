namespace QuanticApi.Business.Contracts;

public sealed record UpdateTradingBotRequest(
    string Name,
    string Symbol,
    string Exchange,
    string Strategy,
    decimal Allocation);
