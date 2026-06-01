import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  template: `<span [class]="tone">{{ label }}</span>`,
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  @Input() label = '';
  @Input() tone: 'positive' | 'negative' | 'neutral' | 'buy' | 'sell' = 'neutral';
}
