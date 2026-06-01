import { inject, Injectable } from '@angular/core';
import { catchError, forkJoin, map, Observable, of } from 'rxjs';
import { MOCK_TRADES } from '../data/mock-trading.data';
import { ApiTrade, Instrument, Order, Strategy } from '../models/api.models';
import { DashboardSummary, PerformancePoint, TimeRange, Trade } from '../models/trading.models';
import { QuanticApiService } from './quantic-api.service';

@Injectable({ providedIn: 'root' })
export class TradingDataService {
  private readonly api = inject(QuanticApiService);

  getSummary(range: TimeRange): Observable<DashboardSummary> {
    return this.getTrades(range).pipe(map((trades) => this.summarize(range, trades)));
  }

  getTrades(range: TimeRange): Observable<Trade[]> {
    const from = this.getCutoff(range).toISOString();
    const pageSize = 100;

    return forkJoin({
      trades: this.api.getTrades({ from, pageSize }),
      instruments: this.api.getInstruments({ pageSize }),
      orders: this.api.getOrders({ pageSize }),
      strategies: this.api.getStrategies({ pageSize })
    }).pipe(
      map(({ trades, instruments, orders, strategies }) =>
        trades.items.map((trade) => this.toUiTrade(trade, instruments.items, orders.items, strategies.items))
      ),
      catchError(() => of(this.getMockTrades(range)))
    );
  }

  getMockSummary(range: TimeRange): DashboardSummary {
    return this.summarize(range, this.getMockTrades(range));
  }

  private summarize(range: TimeRange, trades: Trade[]): DashboardSummary {
    const closedTrades = trades.filter((trade) => trade.status !== 'OPEN');
    const profitableTrades = closedTrades.filter((trade) => trade.pnl > 0);
    const totalProfit = profitableTrades.reduce((sum, trade) => sum + trade.pnl, 0);
    const totalLoss = Math.abs(closedTrades.filter((trade) => trade.pnl < 0).reduce((sum, trade) => sum + trade.pnl, 0));

    return {
      totalPnl: trades.reduce((sum, trade) => sum + trade.pnl, 0),
      pnlChange: 0,
      winRate: closedTrades.length ? (profitableTrades.length / closedTrades.length) * 100 : 0,
      totalTrades: trades.length,
      activeTrades: trades.filter((trade) => trade.status === 'OPEN').length,
      profitFactor: totalLoss ? totalProfit / totalLoss : totalProfit,
      performance: this.buildPerformance(range, trades),
      strategies: this.buildStrategyPerformance(trades),
      trades
    };
  }

  private toUiTrade(trade: ApiTrade, instruments: Instrument[], orders: Order[], strategies: Strategy[]): Trade {
    const instrument = instruments.find((item) => item.id === trade.instrumentId);
    const order = orders.find((item) => item.id === trade.orderId);
    const strategy = strategies.find((item) => item.id === order?.strategyId);
    const pnl = trade.profitLoss ?? 0;

    return {
      id: `TRD-${trade.id}`,
      symbol: instrument?.symbol ?? `Instrument #${trade.instrumentId}`,
      side: trade.side.toUpperCase() === 'SELL' ? 'SELL' : 'BUY',
      quantity: trade.quantity,
      entryPrice: trade.price,
      pnl,
      status: pnl > 0 ? 'PROFIT' : pnl < 0 ? 'LOSS' : 'OPEN',
      strategy: strategy?.strategyName ?? 'Manual trade',
      executedAt: trade.tradeTime
    };
  }

  private getMockTrades(range: TimeRange): Trade[] {
    const cutoff = this.getCutoff(range);
    return MOCK_TRADES.filter((trade) => new Date(trade.executedAt) >= cutoff);
  }

  private getCutoff(range: TimeRange): Date {
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - (range === 'day' ? 1 : range === 'week' ? 7 : 30));
    return cutoff;
  }

  private buildPerformance(range: TimeRange, trades: Trade[]): PerformancePoint[] {
    const sortedTrades = [...trades].sort((a, b) => Date.parse(a.executedAt) - Date.parse(b.executedAt));
    if (!sortedTrades.length) return [{ label: range === 'day' ? 'Today' : 'Start', pnl: 0 }];

    let cumulativePnl = 0;
    return [
      { label: 'Start', pnl: 0 },
      ...sortedTrades.map((trade) => {
        cumulativePnl += trade.pnl;
        const date = new Date(trade.executedAt);
        return {
          label: range === 'day'
            ? date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
            : date.toLocaleDateString([], { day: '2-digit', month: 'short' }),
          pnl: cumulativePnl
        };
      })
    ];
  }

  private buildStrategyPerformance(trades: Trade[]) {
    return Object.values(trades.reduce<Record<string, { name: string; profit: number; trades: number; wins: number }>>((strategies, trade) => {
      const strategy = strategies[trade.strategy] ?? { name: trade.strategy, profit: 0, trades: 0, wins: 0 };
      strategy.profit += trade.pnl;
      strategy.trades += 1;
      strategy.wins += trade.pnl > 0 ? 1 : 0;
      strategies[trade.strategy] = strategy;
      return strategies;
    }, {})).map(({ name, profit, trades: totalTrades, wins }) => ({
      name,
      profit,
      trades: totalTrades,
      success: totalTrades ? (wins / totalTrades) * 100 : 0
    }));
  }
}
