import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-ui-input',
  imports: [FormsModule, MatFormFieldModule, MatInputModule],
  template: `<mat-form-field appearance="outline"><mat-label>{{ label }}</mat-label><input matInput [type]="type" [placeholder]="placeholder" [(ngModel)]="value" /></mat-form-field>`,
  styles: [':host { display: block; } mat-form-field { width: 100%; }']
})
export class UiInputComponent {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() type: 'text' | 'date' | 'search' = 'text';
  @Input() value = '';
}
