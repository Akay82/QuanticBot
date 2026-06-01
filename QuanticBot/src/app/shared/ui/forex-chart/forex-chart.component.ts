import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, Input, OnChanges, OnDestroy, PLATFORM_ID, SimpleChanges, ViewChild } from '@angular/core';
import {
  AreaSeries, CandlestickSeries, ColorType, createChart, IChartApi, LineSeries, UTCTimestamp
} from 'lightweight-charts';
import { ForexChart } from '../../../core/models/api.models';

export type ForexChartMode = 'candlestick' | 'line' | 'area';

@Component({
  selector: 'app-forex-chart',
  template: `<div #chartContainer class="chart-container" aria-label="Forex price chart"></div>`,
  styleUrl: './forex-chart.component.scss'
})
export class ForexChartComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() data: ForexChart | null = null;
  @Input() mode: ForexChartMode = 'candlestick';
  @ViewChild('chartContainer') private chartContainer?: ElementRef<HTMLDivElement>;

  private readonly platformId = inject(PLATFORM_ID);
  private chart?: IChartApi;
  private resizeObserver?: ResizeObserver;

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId) || !this.chartContainer) return;

    this.resizeObserver = new ResizeObserver(() => this.renderChart());
    this.resizeObserver.observe(this.chartContainer.nativeElement);
    this.renderChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] || changes['mode']) this.renderChart();
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.chart?.remove();
  }

  private renderChart(): void {
    if (!isPlatformBrowser(this.platformId) || !this.chartContainer || !this.data?.candles.length) return;

    this.chart?.remove();
    const container = this.chartContainer.nativeElement;
    this.chart = createChart(container, {
      width: container.clientWidth,
      height: 430,
      layout: { background: { type: ColorType.Solid, color: '#0e211f' }, textColor: '#78928e' },
      grid: { vertLines: { color: '#18312e' }, horzLines: { color: '#18312e' } },
      rightPriceScale: { borderColor: '#28423f' },
      timeScale: { borderColor: '#28423f', timeVisible: true, secondsVisible: false }
    });

    const candles = [...this.data.candles]
      .sort((left, right) => Date.parse(left.candleTime) - Date.parse(right.candleTime))
      .map((candle) => ({
        time: Math.floor(Date.parse(candle.candleTime) / 1000) as UTCTimestamp,
        open: candle.open,
        high: candle.high,
        low: candle.low,
        close: candle.close,
        value: candle.close
      }));
    const priceFormat = this.data.symbol.includes('JPY')
      ? { type: 'price' as const, precision: 3, minMove: 0.001 }
      : { type: 'price' as const, precision: 5, minMove: 0.00001 };

    if (this.mode === 'candlestick') {
      const series = this.chart.addSeries(CandlestickSeries, {
        upColor: '#25c98b', downColor: '#f56f7d', wickUpColor: '#25c98b', wickDownColor: '#f56f7d',
        borderUpColor: '#25c98b', borderDownColor: '#f56f7d', priceFormat
      });
      series.setData(candles);
    } else if (this.mode === 'line') {
      const series = this.chart.addSeries(LineSeries, { color: '#58d7ad', lineWidth: 2, priceFormat });
      series.setData(candles);
    } else {
      const series = this.chart.addSeries(AreaSeries, {
        lineColor: '#58d7ad', topColor: 'rgba(37, 201, 139, .35)', bottomColor: 'rgba(37, 201, 139, .02)',
        lineWidth: 2, priceFormat
      });
      series.setData(candles);
    }

    this.chart.timeScale().fitContent();
  }
}
