using QuanticApi.Business.Contracts;
using QuanticApi.Business.Exceptions;

namespace QuanticApi.Business.Services;

public static class RiskManagerService
{
    public static RiskCalculation Calculate(
        decimal availableCapital,
        decimal entryPrice,
        decimal atr,
        decimal riskPercent,
        decimal atrStopMultiplier,
        decimal atrTakeProfitMultiplier)
    {
        var stopDistance = atr * atrStopMultiplier;
        if (availableCapital <= 0 || entryPrice <= 0 || stopDistance <= 0 || riskPercent <= 0)
        {
            throw new BusinessRuleException("Risk calculation requires positive capital, entry price, ATR, and risk percent.");
        }

        var riskAmount = availableCapital * riskPercent / 100;
        var riskSizedQuantity = riskAmount / stopDistance;
        var affordableQuantity = availableCapital / entryPrice;

        return new RiskCalculation(
            Math.Round(Math.Min(riskSizedQuantity, affordableQuantity), 6),
            Math.Round(entryPrice - stopDistance, 6),
            Math.Round(entryPrice + atr * atrTakeProfitMultiplier, 6));
    }
}
