using QuanticApi.Business.Models;

namespace QuanticApi.Business.Contracts;

public sealed record TradingBotResponse(
    Guid Id,
    string Name,
    string Symbol,
    string Exchange,
    string Strategy,
    decimal Allocation,
    TradingBotStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
