import { CurrencyPipe, DatePipe, DecimalPipe, isPlatformBrowser } from '@angular/common';
import { Component, DestroyRef, NgZone, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin, Observable } from 'rxjs';
import {
  BotLog, CreateTradingBotRequest, ForexChart, Instrument, PaperAccount, SwingBotDashboard,
  SwingBotSettingsRequest, TradingBot
} from '../../core/models/api.models';
import { QuanticApiService } from '../../core/services/quantic-api.service';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { StatCardComponent } from '../../shared/ui/stat-card/stat-card.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiPanelComponent } from '../../shared/ui/ui-panel/ui-panel.component';

@Component({
  selector: 'app-bots',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, StatusBadgeComponent, StatCardComponent, UiButtonComponent, UiPanelComponent],
  templateUrl: './bots.component.html',
  styleUrl: './bots.component.scss'
})
export class BotsComponent {
  private readonly api = inject(QuanticApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngZone = inject(NgZone);
  private readonly platformId = inject(PLATFORM_ID);
  private eventSource?: EventSource;

  protected readonly bots = signal<TradingBot[]>([]);
  protected readonly accounts = signal<PaperAccount[]>([]);
  protected readonly instruments = signal<Instrument[]>([]);
  protected readonly selectedBotId = signal<string | null>(null);
  protected readonly dashboard = signal<SwingBotDashboard | null>(null);
  protected readonly logs = signal<BotLog[]>([]);
  protected readonly marketCharts = signal<ForexChart[]>([]);
  protected readonly loading = signal(false);
  protected readonly actionLoading = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected botName = 'EUR USD Swing Bot';
  protected botSymbol = 'EUR/USD';
  protected botAllocation = 1000;
  protected settings: SwingBotSettingsRequest = this.defaultSettings();

  protected readonly selectedBot = computed(() => this.bots().find((bot) => bot.id === this.selectedBotId()) ?? null);
  protected readonly activePosition = computed(() => this.dashboard()?.position ?? null);
  protected readonly latestLog = computed(() => this.logs()[0] ?? null);
  protected readonly currentQuote = computed(() => {
    const instrumentId = this.dashboard()?.settings?.instrumentId;
    return this.marketCharts().find((chart) => chart.instrumentId === instrumentId)?.candles.at(-1)?.close ?? null;
  });
  protected readonly openPnl = computed(() => {
    const position = this.activePosition();
    const quote = this.currentQuote();
    if (!position || quote === null) return 0;
    const quotePnl = (quote - position.entryPrice) * position.quantity;
    const settings = this.dashboard()?.settings;
    const instrument = this.instruments().find((item) => item.id === settings?.instrumentId);
    const accountCurrency = this.accounts().find((account) => account.id === settings?.accountId)?.currency;
    if (!instrument?.quoteCurrency || !accountCurrency || instrument.quoteCurrency === accountCurrency) return quotePnl;
    if (instrument.baseCurrency === accountCurrency) return quotePnl / quote;
    const directRate = this.latestRate(`${instrument.quoteCurrency}/${accountCurrency}`);
    if (directRate) return quotePnl * directRate;
    const inverseRate = this.latestRate(`${accountCurrency}/${instrument.quoteCurrency}`);
    return inverseRate ? quotePnl / inverseRate : quotePnl;
  });

  constructor() {
    this.loadInitialData();
    this.startDashboardPolling();
    this.destroyRef.onDestroy(() => this.eventSource?.close());
  }

  protected createBot(): void {
    if (!this.botName.trim() || !this.botSymbol || this.botAllocation <= 0) {
      this.error.set('Enter a bot name, symbol, and allocation greater than zero.');
      return;
    }
    const request: CreateTradingBotRequest = {
      name: this.botName.trim(),
      symbol: this.botSymbol,
      exchange: 'TWELVE_DATA',
      strategy: 'EmaRsiSwingBot',
      allocation: this.botAllocation
    };
    this.runAction(this.api.createTradingBot(request), 'Swing bot created. Configure its settings before starting.', (bot) => {
      this.bots.update((items) => [...items, bot as TradingBot]);
      this.selectBot((bot as TradingBot).id);
    });
  }

  protected selectBot(botId: string): void {
    this.selectedBotId.set(botId);
    this.loadDashboard();
    this.connectLogStream();
  }

  protected saveSettings(): void {
    const botId = this.selectedBotId();
    if (!botId || !this.settings.accountId || !this.settings.instrumentId) {
      this.error.set('Select a bot, paper account, and forex instrument.');
      return;
    }
    this.runAction(this.api.updateSwingBotSettings(botId, this.settings), 'EMA RSI swing settings saved.', () => this.loadDashboard());
  }

  protected startBot(): void {
    const botId = this.selectedBotId();
    if (botId) this.runAction(this.api.startTradingBot(botId), 'Bot started.', () => this.reloadSelectedBot());
  }

  protected pauseBot(): void {
    const botId = this.selectedBotId();
    if (botId) this.runAction(this.api.pauseTradingBot(botId), 'Bot paused.', () => this.reloadSelectedBot());
  }

  protected stopBot(): void {
    const botId = this.selectedBotId();
    if (botId) this.runAction(this.api.stopTradingBot(botId), 'Bot stopped.', () => this.reloadSelectedBot());
  }

  protected evaluateBot(): void {
    const botId = this.selectedBotId();
    if (!botId) return;
    this.runAction(this.api.evaluateTradingBot(botId), 'Bot evaluation requested.', (response) => {
      const result = response as { outcome: string; message: string };
      this.message.set(`${result.outcome}: ${result.message}`);
      this.loadDashboard();
    });
  }

  protected botStatus(bot: TradingBot | null): string { return bot?.status === 1 ? 'RUNNING' : 'PAUSED'; }
  protected botTone(bot: TradingBot | null): 'positive' | 'neutral' { return bot?.status === 1 ? 'positive' : 'neutral'; }
  protected instrumentName(instrumentId: number): string { return this.instruments().find((item) => item.id === instrumentId)?.symbol ?? `Instrument #${instrumentId}`; }

  protected loadInitialData(): void {
    this.loading.set(true);
    forkJoin({
      bots: this.api.getTradingBots(),
      accounts: this.api.getPaperAccounts({ isActive: true, pageSize: 100 }),
      instruments: this.api.getInstruments({ exchange: 'TWELVE_DATA', isActive: true, pageSize: 100 }),
      charts: this.api.getForexCharts({ timeframe: '1m', candleCount: 1 })
    }).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ bots, accounts, instruments, charts }) => {
        this.bots.set(bots);
        this.accounts.set(accounts.items);
        this.instruments.set(instruments.items);
        this.marketCharts.set(charts);
        this.settings = {
          ...this.settings,
          accountId: accounts.items[0]?.id ?? 0,
          instrumentId: instruments.items[0]?.id ?? 0
        };
        if (bots[0]) this.selectBot(bots[0].id);
      },
      error: () => this.error.set('Unable to load trading-bot resources. Confirm that the swing-bot SQL script has been applied.')
    });
  }

  private loadDashboard(): void {
    const botId = this.selectedBotId();
    if (!botId) return;
    this.api.getSwingBotDashboard(botId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.logs.set(dashboard.recentLogs);
        if (dashboard.settings) this.settings = { ...dashboard.settings };
      },
      error: () => this.error.set('Unable to load the selected bot dashboard.')
    });
  }

  private reloadSelectedBot(): void {
    this.api.getTradingBots().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (bots) => { this.bots.set(bots); this.loadDashboard(); }
    });
  }

  private startDashboardPolling(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.ngZone.runOutsideAngular(() => {
      const timer = window.setInterval(() => this.ngZone.run(() => { this.loadDashboard(); this.loadMarketQuotes(); }), 15000);
      this.destroyRef.onDestroy(() => window.clearInterval(timer));
    });
  }

  private loadMarketQuotes(): void {
    this.api.getForexCharts({ timeframe: '1m', candleCount: 1 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (charts) => this.marketCharts.set(charts) });
  }

  private latestRate(symbol: string): number | null {
    return this.marketCharts().find((chart) => chart.symbol === symbol)?.candles.at(-1)?.close ?? null;
  }

  private connectLogStream(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.eventSource?.close();
    const botId = this.selectedBotId();
    if (!botId) return;

    this.eventSource = new EventSource(this.api.tradingBotLogStreamUrl(botId));
    this.eventSource.onmessage = (event) => {
      const data = JSON.parse(event.data) as BotLog & Record<string, unknown>;
      const log: BotLog = {
        id: (data.id ?? data['Id']) as number,
        level: (data.level ?? data['Level']) as string,
        eventType: (data.eventType ?? data['EventType']) as string,
        message: (data.message ?? data['Message']) as string,
        details: (data.details ?? data['Details']) as string | null | undefined,
        createdAtUtc: (data.createdAtUtc ?? data['CreatedAtUtc']) as string
      };
      this.ngZone.run(() => this.logs.update((logs) => [log, ...logs.filter((item) => item.id !== log.id)].slice(0, 100)));
    };
  }

  private runAction(request: Observable<unknown>, successMessage: string, next?: (response: unknown) => void): void {
    this.actionLoading.set(true);
    this.message.set('');
    this.error.set('');
    request.pipe(finalize(() => this.actionLoading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => { this.message.set(successMessage); next?.(response); },
      error: () => this.error.set('The bot action failed. Check the settings and backend logs for details.')
    });
  }

  private defaultSettings(): SwingBotSettingsRequest {
    return {
      accountId: 0, instrumentId: 0, timeframe: '4h', fastEmaPeriod: 50, slowEmaPeriod: 200,
      rsiPeriod: 14, atrPeriod: 14, rsiEntryMin: 40, rsiEntryMax: 55, rsiExit: 70,
      atrStopMultiplier: 1.5, atrTakeProfitMultiplier: 3, riskPercent: 1
    };
  }
}
