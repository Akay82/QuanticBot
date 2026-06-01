import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Trade } from '../../../core/models/trading.models';
import { StatusBadgeComponent } from '../../ui/status-badge/status-badge.component';

@Component({
  selector: 'app-trade-table',
  imports: [CurrencyPipe, DatePipe, StatusBadgeComponent],
  templateUrl: './trade-table.component.html'
})
export class TradeTableComponent {
  @Input() trades: Trade[] = [];
}
