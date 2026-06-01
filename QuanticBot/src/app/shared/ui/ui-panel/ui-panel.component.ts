import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-ui-panel',
  template: `
    <section>
      <header><div><h2>{{ title }}</h2><p>{{ subtitle }}</p></div><ng-content select="[panel-action]" /></header>
      <ng-content />
    </section>
  `,
  styleUrl: './ui-panel.component.scss'
})
export class UiPanelComponent {
  @Input() title = '';
  @Input() subtitle = '';
}
