import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  protected readonly links = [
    { label: 'Overview', path: '/', icon: 'O', exact: true },
    { label: 'Paper trading', path: '/paper-trading', icon: '+', exact: false },
    { label: 'Forex charts', path: '/market-data', icon: '~', exact: false },
    { label: 'Trades', path: '/trades', icon: 'T', exact: false },
    { label: 'Reports', path: '/reports', icon: 'R', exact: false }
  ];
}
