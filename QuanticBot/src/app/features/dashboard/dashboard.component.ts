import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TimeRange } from '../../core/models/trading.models';
import { TradingDataService } from '../../core/services/trading-data.service';
import { PerformanceChartComponent } from '../../shared/ui/performance-chart/performance-chart.component';
import { StatCardComponent } from '../../shared/ui/stat-card/stat-card.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiPanelComponent } from '../../shared/ui/ui-panel/ui-panel.component';
import { UiSelectComponent } from '../../shared/ui/ui-select/ui-select.component';
import { TradeTableComponent } from '../../shared/components/trade-table/trade-table.component';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DecimalPipe, PerformanceChartComponent, StatCardComponent, TradeTableComponent, UiButtonComponent, UiPanelComponent, UiSelectComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly data = inject(TradingDataService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly range = signal<TimeRange>('week');
  protected readonly summary = signal(this.data.getMockSummary(this.range()));

  constructor() {
    this.loadSummary();
  }

  protected updateRange(range: TimeRange): void {
    this.range.set(range);
    this.loadSummary();
  }

  private loadSummary(): void {
    this.data.getSummary(this.range())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((summary) => this.summary.set(summary));
  }
}
