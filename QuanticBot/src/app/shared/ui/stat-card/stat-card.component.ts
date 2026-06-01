import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  template: `
    <article>
      <div class="top"><span class="label">{{ label }}</span><span class="icon">{{ icon }}</span></div>
      <strong>{{ value }}</strong>
      <span class="caption" [class.positive]="positive">{{ caption }}</span>
    </article>
  `,
  styleUrl: './stat-card.component.scss'
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value: string | null = '';
  @Input() caption = '';
  @Input() icon = '↗';
  @Input() positive = false;
}
