import { CurrencyPipe, DatePipe, DecimalPipe, isPlatformBrowser } from '@angular/common';
import { Component, DestroyRef, NgZone, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin, Observable, switchMap } from 'rxjs';
import { ApiTrade, CreateOrderRequest, ForexChart, Instrument, Order, PaperAccount, Position } from '../../core/models/api.models';
import { QuanticApiService } from '../../core/services/quantic-api.service';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiPanelComponent } from '../../shared/ui/ui-panel/ui-panel.component';
import { StatCardComponent } from '../../shared/ui/stat-card/stat-card.component';

@Component({
  selector: 'app-paper-trading',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, StatCardComponent, StatusBadgeComponent, UiButtonComponent, UiPanelComponent],
  templateUrl: './paper-trading.component.html',
  styleUrl: './paper-trading.component.scss'
})
export class PaperTradingComponent {
  private readonly api = inject(QuanticApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngZone = inject(NgZone);
  private readonly platformId = inject(PLATFORM_ID);

  protected readonly accounts = signal<PaperAccount[]>([]);
  protected readonly instruments = signal<Instrument[]>([]);
  protected readonly orders = signal<Order[]>([]);
  protected readonly positions = signal<Position[]>([]);
  protected readonly trades = signal<ApiTrade[]>([]);
  protected readonly marketCharts = signal<ForexChart[]>([]);
  protected readonly loading = signal(false);
  protected readonly actionLoading = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly executionPrices: Record<number, number> = {};

  protected accountId: number | null = null;
  protected instrumentId: number | null = null;
  protected side: 'BUY' | 'SELL' = 'BUY';
  protected quantity = 100;
  protected requestedPrice: number | null = null;

  protected readonly selectedAccount = computed(() => this.accounts().find((account) => account.id === this.accountId) ?? null);
  protected readonly pendingOrders = computed(() => this.orders().filter((order) => order.status === 'PENDING'));
  protected readonly realizedPnl = computed(() => this.trades().reduce((sum, trade) => sum + (trade.profitLoss ?? 0), 0));
  protected readonly openPositions = computed(() => this.positions().filter((position) => position.quantity > 0));
  protected readonly unrealizedPnl = computed(() => this.openPositions().reduce((sum, position) => sum + this.positionPnl(position), 0));

  constructor() {
    this.loadInitialData();
    this.startQuotePolling();
  }

  protected changeAccount(): void {
    if (this.accountId) this.loadAccountData();
  }

  protected placeOrder(): void {
    if (!this.accountId || !this.instrumentId || this.quantity <= 0) {
      this.error.set('Select an account, instrument, and quantity greater than zero.');
      return;
    }

    const request: CreateOrderRequest = {
      accountId: this.accountId,
      strategyId: null,
      instrumentId: this.instrumentId,
      orderType: 'MARKET',
      side: this.side,
      quantity: this.quantity,
      requestedPrice: this.requestedPrice,
      status: 'PENDING'
    };

    this.runAction(this.api.createOrder(request), `${this.side} order created. Execute it when the price is ready.`);
  }

  protected execute(order: Order): void {
    const executedPrice = this.executionPrices[order.id] ?? order.requestedPrice;
    if (!executedPrice || executedPrice <= 0) {
      this.error.set('Enter a valid execution price before executing the order.');
      return;
    }
    this.runAction(this.api.executeOrder(order.id, executedPrice), `Order #${order.id} executed.`);
  }

  protected cancel(order: Order): void {
    this.runAction(this.api.cancelOrder(order.id), `Order #${order.id} cancelled.`);
  }

  protected closePosition(position: Position): void {
    const currentPrice = this.positionCurrentPrice(position);
    if (!this.accountId || !currentPrice || position.quantity <= 0) {
      this.error.set('This position cannot be closed because its current price is unavailable.');
      return;
    }

    const closeOrder: CreateOrderRequest = {
      accountId: this.accountId,
      strategyId: null,
      instrumentId: position.instrumentId,
      orderType: 'MARKET',
      side: 'SELL',
      quantity: position.quantity,
      requestedPrice: currentPrice,
      status: 'PENDING'
    };

    this.actionLoading.set(true);
    this.clearFeedback();
    this.api.createOrder(closeOrder).pipe(
      switchMap((order) => this.api.executeOrder(order.id, currentPrice)),
      finalize(() => this.actionLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => { this.message.set('Position closed successfully.'); this.loadAccountData(); },
      error: () => this.error.set('Unable to close the position. Confirm that the account holds enough quantity.')
    });
  }

  protected instrumentName(instrumentId: number): string {
    return this.instruments().find((instrument) => instrument.id === instrumentId)?.symbol ?? `Instrument #${instrumentId}`;
  }

  protected positionCurrentPrice(position: Position): number | null {
    return this.marketCharts().find((chart) => chart.instrumentId === position.instrumentId)?.candles.at(-1)?.close
      ?? position.currentPrice
      ?? null;
  }

  protected positionPnl(position: Position): number {
    const currentPrice = this.positionCurrentPrice(position);
    if (currentPrice === null) return position.unrealizedPnl ?? 0;

    const quotePnl = (currentPrice - position.averagePrice) * position.quantity;
    const instrument = this.instruments().find((item) => item.id === position.instrumentId);
    const accountCurrency = this.selectedAccount()?.currency;
    if (!instrument?.quoteCurrency || !accountCurrency || instrument.quoteCurrency === accountCurrency) return quotePnl;
    if (instrument.baseCurrency === accountCurrency) return quotePnl / currentPrice;

    const directRate = this.latestRate(`${instrument.quoteCurrency}/${accountCurrency}`);
    if (directRate) return quotePnl * directRate;
    const inverseRate = this.latestRate(`${accountCurrency}/${instrument.quoteCurrency}`);
    return inverseRate ? quotePnl / inverseRate : quotePnl;
  }

  private loadInitialData(): void {
    this.loading.set(true);
    forkJoin({
      accounts: this.api.getPaperAccounts({ isActive: true, pageSize: 100 }),
      instruments: this.api.getInstruments({ marketType: 'FOREX', isActive: true, pageSize: 100 })
    }).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ accounts, instruments }) => {
        this.accounts.set(accounts.items);
        this.instruments.set(instruments.items);
        this.accountId = accounts.items[0]?.id ?? null;
        this.instrumentId = instruments.items[0]?.id ?? null;
        if (this.accountId) this.loadAccountData();
      },
      error: () => this.error.set('Unable to load paper-trading resources.')
    });
  }

  private loadAccountData(): void {
    if (!this.accountId) return;
    const accountId = this.accountId;
    this.loading.set(true);
    forkJoin({
      orders: this.api.getOrders({ accountId, pageSize: 100 }),
      positions: this.api.getPositions({ accountId, pageSize: 100 }),
      trades: this.api.getTrades({ accountId, pageSize: 100 }),
      charts: this.api.getForexCharts({ timeframe: '1m', candleCount: 1 })
    }).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ orders, positions, trades, charts }) => {
        this.orders.set(orders.items);
        this.positions.set(positions.items);
        this.trades.set(trades.items);
        this.marketCharts.set(charts);
        for (const order of orders.items) {
          if (order.requestedPrice) this.executionPrices[order.id] ??= order.requestedPrice;
        }
      },
      error: () => this.error.set('Unable to load account trading data.')
    });
  }

  private startQuotePolling(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    this.ngZone.runOutsideAngular(() => {
      const timer = window.setInterval(() => this.refreshMarketQuotes(), 15000);
      this.destroyRef.onDestroy(() => window.clearInterval(timer));
    });
  }

  private refreshMarketQuotes(): void {
    this.api.getForexCharts({ timeframe: '1m', candleCount: 1 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (charts) => this.ngZone.run(() => this.marketCharts.set(charts)) });
  }

  private latestRate(symbol: string): number | null {
    return this.marketCharts().find((chart) => chart.symbol === symbol)?.candles.at(-1)?.close ?? null;
  }

  private runAction(request: Observable<unknown>, successMessage: string): void {
    this.actionLoading.set(true);
    this.clearFeedback();
    request.pipe(finalize(() => this.actionLoading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.message.set(successMessage); this.loadAccountData(); },
      error: () => this.error.set('The trading action failed. Review the order details and try again.')
    });
  }

  private clearFeedback(): void {
    this.message.set('');
    this.error.set('');
  }
}
