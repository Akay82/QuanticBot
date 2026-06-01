import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { ForexChart } from '../../core/models/api.models';
import { QuanticApiService } from '../../core/services/quantic-api.service';
import { ForexChartComponent, ForexChartMode } from '../../shared/ui/forex-chart/forex-chart.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiPanelComponent } from '../../shared/ui/ui-panel/ui-panel.component';

@Component({
  selector: 'app-market-data',
  imports: [DatePipe, DecimalPipe, ForexChartComponent, UiButtonComponent, UiPanelComponent],
  templateUrl: './market-data.component.html',
  styleUrl: './market-data.component.scss'
})
export class MarketDataComponent {
  private readonly api = inject(QuanticApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly charts = signal<ForexChart[]>([]);
  protected readonly selectedInstrumentId = signal<number | null>(null);
  protected readonly timeframe = signal('1m');
  protected readonly candleCount = signal(120);
  protected readonly chartMode = signal<ForexChartMode>('candlestick');
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly timeframes = ['1m', '5m', '15m', '1h', '4h', '1d'];
  protected readonly modes: { label: string; value: ForexChartMode }[] = [
    { label: 'Candles', value: 'candlestick' },
    { label: 'Line', value: 'line' },
    { label: 'Area', value: 'area' }
  ];
  protected readonly selectedChart = computed(() =>
    this.charts().find((chart) => chart.instrumentId === this.selectedInstrumentId()) ?? this.charts()[0] ?? null
  );
  protected readonly latestCandle = computed(() => this.selectedChart()?.candles.at(-1) ?? null);
  protected readonly priceChange = computed(() => {
    const candles = this.selectedChart()?.candles ?? [];
    if (candles.length < 2) return 0;
    return candles.at(-1)!.close - candles.at(-2)!.close;
  });

  constructor() {
    this.loadCharts();
  }

  protected selectInstrument(instrumentId: number): void { this.selectedInstrumentId.set(instrumentId); }
  protected selectMode(mode: ForexChartMode): void { this.chartMode.set(mode); }
  protected selectTimeframe(timeframe: string): void { this.timeframe.set(timeframe); this.loadCharts(); }

  protected refresh(): void {
    this.loading.set(true);
    this.api.refreshForexMarketData()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: () => this.loadCharts(), error: () => { this.error.set('Unable to refresh market data.'); this.loading.set(false); } });
  }

  private loadCharts(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.getForexCharts({ timeframe: this.timeframe(), candleCount: this.candleCount() })
      .pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (charts) => {
          this.charts.set(charts);
          if (!charts.some((chart) => chart.instrumentId === this.selectedInstrumentId())) this.selectedInstrumentId.set(charts[0]?.instrumentId ?? null);
        },
        error: () => this.error.set('Market charts are temporarily unavailable.')
      });
  }
}
