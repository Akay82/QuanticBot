import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TimeRange } from '../../core/models/trading.models';
import { TradingDataService } from '../../core/services/trading-data.service';
import { TradeTableComponent } from '../../shared/components/trade-table/trade-table.component';
import { UiButtonComponent } from '../../shared/ui/ui-button/ui-button.component';
import { UiInputComponent } from '../../shared/ui/ui-input/ui-input.component';
import { UiPanelComponent } from '../../shared/ui/ui-panel/ui-panel.component';
import { UiSelectComponent } from '../../shared/ui/ui-select/ui-select.component';

@Component({
  selector: 'app-trades',
  imports: [TradeTableComponent, UiButtonComponent, UiInputComponent, UiPanelComponent, UiSelectComponent],
  templateUrl: './trades.component.html',
  styleUrl: './trades.component.scss'
})
export class TradesComponent {
  private readonly data = inject(TradingDataService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly range = signal<TimeRange>('month');
  protected readonly trades = signal(this.data.getMockSummary(this.range()).trades);

  constructor() {
    this.loadTrades();
  }

  protected updateRange(range: TimeRange): void {
    this.range.set(range);
    this.loadTrades();
  }

  private loadTrades(): void {
    this.data.getTrades(this.range())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((trades) => this.trades.set(trades));
  }
}
