export type TimeRange = 'day' | 'week' | 'month';
export type TradeSide = 'BUY' | 'SELL';
export type TradeStatus = 'PROFIT' | 'LOSS' | 'OPEN';

export interface Trade {
  id: string;
  symbol: string;
  side: TradeSide;
  quantity: number;
  entryPrice: number;
  exitPrice?: number;
  pnl: number;
  status: TradeStatus;
  strategy: string;
  executedAt: string;
}

export interface PerformancePoint {
  label: string;
  pnl: number;
}

export interface StrategyPerformance {
  name: string;
  profit: number;
  trades: number;
  success: number;
}

export interface DashboardSummary {
  totalPnl: number;
  pnlChange: number;
  winRate: number;
  totalTrades: number;
  activeTrades: number;
  profitFactor: number;
  performance: PerformancePoint[];
  strategies: StrategyPerformance[];
  trades: Trade[];
}
