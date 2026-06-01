import { Component, Input } from '@angular/core';
import { PerformancePoint } from '../../../core/models/trading.models';

@Component({
  selector: 'app-performance-chart',
  template: `
    <div class="chart">
      <div class="scale"><span>₹40k</span><span>₹30k</span><span>₹20k</span><span>₹10k</span><span>₹0</span></div>
      <svg viewBox="0 0 680 210" preserveAspectRatio="none" role="img" aria-label="Profit and loss performance chart">
        <defs><linearGradient id="area" x1="0" x2="0" y1="0" y2="1"><stop stop-color="#23c98a" stop-opacity=".35" /><stop offset="1" stop-color="#23c98a" stop-opacity="0" /></linearGradient></defs>
        <path class="area" [attr.d]="areaPath" /><path class="line" [attr.d]="linePath" />
      </svg>
      <div class="labels">@for (point of points; track point.label) { <span>{{ point.label }}</span> }</div>
    </div>
  `,
  styleUrl: './performance-chart.component.scss'
})
export class PerformanceChartComponent {
  @Input() points: PerformancePoint[] = [];

  get linePath(): string { return this.path(false); }
  get areaPath(): string { return this.path(true); }

  private path(close: boolean): string {
    if (!this.points.length) return '';
    const width = 680;
    const height = 185;
    const max = Math.max(40000, ...this.points.map((point) => point.pnl));
    const coords = this.points.map((point, index) => {
      const x = this.points.length === 1 ? 0 : (index / (this.points.length - 1)) * width;
      const y = height - (point.pnl / max) * (height - 22);
      return `${index ? 'L' : 'M'} ${x} ${y}`;
    }).join(' ');
    return close ? `${coords} L ${width} ${height} L 0 ${height} Z` : coords;
  }
}
