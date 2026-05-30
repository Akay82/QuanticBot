namespace QuanticApi.Business.Contracts;

public sealed record CreateTradingBotRequest(
    string Name,
    string Symbol,
    string Exchange,
    string Strategy,
    decimal Allocation);
