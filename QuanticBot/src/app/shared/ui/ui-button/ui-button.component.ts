import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-ui-button',
  imports: [MatButtonModule],
  template: `
    <button mat-flat-button [class.secondary]="variant === 'secondary'" [class.ghost]="variant === 'ghost'" [type]="type" [disabled]="disabled" (click)="pressed.emit()">
      @if (icon) { <span class="icon">{{ icon }}</span> }
      <ng-content />
    </button>
  `,
  styleUrl: './ui-button.component.scss'
})
export class UiButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'ghost' = 'primary';
  @Input() icon = '';
  @Input() type: 'button' | 'submit' = 'button';
  @Input() disabled = false;
  @Output() pressed = new EventEmitter<void>();
}
