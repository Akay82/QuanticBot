import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../api/api-endpoints';
import {
  ApiTrade, BotEvaluationResponse, BotLog, CreateOrderRequest, CreateTradingBotRequest, ForexChart,
  Instrument, MarketDataRefreshResponse, Order, PagedResponse, PaperAccount, Position, PriceHistory,
  QueryParams, Signal, Strategy, SwingBotDashboard, SwingBotSettings, SwingBotSettingsRequest, TradingBot, Watchlist
} from '../models/api.models';
import { CrudApiService } from './crud-api.service';

@Injectable({ providedIn: 'root' })
export class QuanticApiService {
  private readonly crud = inject(CrudApiService);
  private readonly http = inject(HttpClient);

  getInstruments(query?: QueryParams): Observable<PagedResponse<Instrument>> { return this.crud.getPage(API_ENDPOINTS.instruments.list, query); }
  getPaperAccounts(query?: QueryParams): Observable<PagedResponse<PaperAccount>> { return this.crud.getPage(API_ENDPOINTS.paperAccounts.list, query); }
  getStrategies(query?: QueryParams): Observable<PagedResponse<Strategy>> { return this.crud.getPage(API_ENDPOINTS.strategies.list, query); }
  getOrders(query?: QueryParams): Observable<PagedResponse<Order>> { return this.crud.getPage(API_ENDPOINTS.orders.list, query); }
  createOrder(order: CreateOrderRequest): Observable<Order> { return this.http.post<Order>(API_ENDPOINTS.orders.list, order); }
  getPositions(query?: QueryParams): Observable<PagedResponse<Position>> { return this.crud.getPage(API_ENDPOINTS.positions.list, query); }
  getPriceHistory(query?: QueryParams): Observable<PagedResponse<PriceHistory>> { return this.crud.getPage(API_ENDPOINTS.priceHistory.list, query); }
  getSignals(query?: QueryParams): Observable<PagedResponse<Signal>> { return this.crud.getPage(API_ENDPOINTS.signals.list, query); }
  getTrades(query?: QueryParams): Observable<PagedResponse<ApiTrade>> { return this.crud.getPage(API_ENDPOINTS.trades.list, query); }
  getWatchlists(query?: QueryParams): Observable<PagedResponse<Watchlist>> { return this.crud.getPage(API_ENDPOINTS.watchlists.list, query); }
  getForexCharts(query?: Record<string, string | number | boolean>): Observable<ForexChart[]> { return this.http.get<ForexChart[]>(API_ENDPOINTS.marketData.forexCharts, { params: query }); }
  getForexChart(instrumentId: number, query?: Record<string, string | number | boolean>): Observable<ForexChart> { return this.http.get<ForexChart>(API_ENDPOINTS.marketData.forexChart(instrumentId), { params: query }); }
  refreshForexMarketData(): Observable<MarketDataRefreshResponse> { return this.http.post<MarketDataRefreshResponse>(API_ENDPOINTS.marketData.refreshForex, {}); }

  cancelOrder(id: number): Observable<void> { return this.http.post<void>(API_ENDPOINTS.orders.cancel(id), {}); }
  executeOrder(id: number, executedPrice: number): Observable<void> { return this.http.post<void>(API_ENDPOINTS.orders.execute(id), { executedPrice }); }

  getTradingBots(): Observable<TradingBot[]> { return this.http.get<TradingBot[]>(API_ENDPOINTS.tradingBots.list); }
  createTradingBot(request: CreateTradingBotRequest): Observable<TradingBot> { return this.http.post<TradingBot>(API_ENDPOINTS.tradingBots.list, request); }
  updateSwingBotSettings(id: string, request: SwingBotSettingsRequest): Observable<SwingBotSettings> { return this.http.put<SwingBotSettings>(API_ENDPOINTS.tradingBots.settings(id), request); }
  getSwingBotDashboard(id: string): Observable<SwingBotDashboard> { return this.http.get<SwingBotDashboard>(API_ENDPOINTS.tradingBots.dashboard(id)); }
  getTradingBotLogs(id: string, count = 100): Observable<BotLog[]> { return this.http.get<BotLog[]>(API_ENDPOINTS.tradingBots.logs(id), { params: { count } }); }
  tradingBotLogStreamUrl(id: string): string { return API_ENDPOINTS.tradingBots.logsStream(id); }
  evaluateTradingBot(id: string): Observable<BotEvaluationResponse> { return this.http.post<BotEvaluationResponse>(API_ENDPOINTS.tradingBots.evaluate(id), {}); }
  startTradingBot(id: string): Observable<TradingBot> { return this.http.post<TradingBot>(API_ENDPOINTS.tradingBots.start(id), {}); }
  pauseTradingBot(id: string): Observable<TradingBot> { return this.http.post<TradingBot>(API_ENDPOINTS.tradingBots.pause(id), {}); }
  stopTradingBot(id: string): Observable<TradingBot> { return this.http.post<TradingBot>(API_ENDPOINTS.tradingBots.stop(id), {}); }
}
