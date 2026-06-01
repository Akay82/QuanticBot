import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { TimeRange } from '../../../core/models/trading.models';

@Component({
  selector: 'app-ui-select',
  imports: [MatButtonToggleModule],
  template: `
    <mat-button-toggle-group [value]="value" (change)="valueChange.emit($event.value)" aria-label="Select date range">
      @for (option of options; track option.value) {
        <mat-button-toggle [value]="option.value">{{ option.label }}</mat-button-toggle>
      }
    </mat-button-toggle-group>
  `,
  styleUrl: './ui-select.component.scss'
})
export class UiSelectComponent {
  @Input() value: TimeRange = 'week';
  @Output() valueChange = new EventEmitter<TimeRange>();
  readonly options = [
    { label: 'Today', value: 'day' },
    { label: 'This week', value: 'week' },
    { label: 'This month', value: 'month' }
  ];
}
