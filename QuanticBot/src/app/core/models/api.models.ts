export interface EntityBase {
  id: number;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type QueryParams = Record<string, string | number | boolean | undefined | null>;

export interface Instrument extends EntityBase {
  symbol: string;
  name?: string | null;
  marketType: string;
  exchange?: string | null;
  baseCurrency?: string | null;
  quoteCurrency?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface PaperAccount extends EntityBase {
  userId: number;
  accountName: string;
  startingBalance: number;
  currentBalance: number;
  currency: string;
  isActive: boolean;
  createdAt: string;
}

export interface Strategy extends EntityBase {
  userId: number;
  strategyName: string;
  description?: string | null;
  strategyType?: string | null;
  parameters?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface Order extends EntityBase {
  accountId: number;
  strategyId?: number | null;
  instrumentId: number;
  orderType: string;
  side: string;
  quantity: number;
  requestedPrice?: number | null;
  executedPrice?: number | null;
  status: string;
  orderTime: string;
  executedTime?: string | null;
}

export interface CreateOrderRequest {
  accountId: number;
  strategyId?: number | null;
  instrumentId: number;
  orderType: string;
  side: string;
  quantity: number;
  requestedPrice?: number | null;
  status: string;
}

export interface Position extends EntityBase {
  accountId: number;
  instrumentId: number;
  quantity: number;
  averagePrice: number;
  currentPrice?: number | null;
  unrealizedPnl?: number | null;
  updatedAt: string;
}

export interface PriceHistory extends EntityBase {
  instrumentId: number;
  timeframe: string;
  openPrice: number;
  highPrice: number;
  lowPrice: number;
  closePrice: number;
  volume?: number | null;
  candleTime: string;
}

export interface Signal extends EntityBase {
  strategyId: number;
  instrumentId: number;
  signalType: string;
  signalPrice?: number | null;
  confidence?: number | null;
  reason?: string | null;
  createdAt: string;
}

export interface ApiTrade extends EntityBase {
  orderId: number;
  accountId: number;
  instrumentId: number;
  side: string;
  quantity: number;
  price: number;
  totalValue: number;
  profitLoss?: number | null;
  tradeTime: string;
}

export interface Watchlist extends EntityBase {
  userId: number;
  instrumentId: number;
  createdAt: string;
}

export interface ForexCandle {
  candleTime: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume?: number | null;
}

export interface ForexChart {
  instrumentId: number;
  symbol: string;
  name: string;
  timeframe: string;
  candles: ForexCandle[];
}

export interface MarketDataRefreshResponse {
  refreshedAtUtc: string;
  instrumentCount: number;
  candleCount: number;
}

export type TradingBotStatus = 0 | 1;

export interface TradingBot {
  id: string;
  name: string;
  symbol: string;
  exchange: string;
  strategy: string;
  allocation: number;
  status: TradingBotStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
}
